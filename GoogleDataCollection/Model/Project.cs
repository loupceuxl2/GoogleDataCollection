using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoogleMapsApi;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using GoogleDataCollection.Logging;
using Newtonsoft.Json;

using System.Diagnostics;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Project : ILog
    {
        public static bool EnableLogging = true;

        public static uint MaxRequests = 50;

        // !IMPORTANT: MaxBatchRequest must be <= MaxRequests.
        public static uint MaxBatchRequests = 45;

        public static uint BatchIntervalTime = 1100;

        [JsonProperty(PropertyName = "apiKey", Required = Required.Always)]
        public string ApiKey { get; set; }

        [JsonProperty(PropertyName = "number", Required = Required.Always)]
        public uint Number { get; set; }

        public UpdateSession CurrentSession { get; set; }

        public ProjectSummary Summary { get; protected set; }

        public Log Log { get; set; }

        public Project()
        {

        }

        public void LoadProject()
        {
            Summary = new ProjectSummary((int)Number);

            Log = new Log(new FileInfo($"{ AppDomain.CurrentDomain.BaseDirectory }\\project_{ Number }.txt"))
            {
                Output = Log.OutputFormats.File,         // Add file output for distinct project logs.
                FileWriteMode = Log.FileWriteModes.Overwrite,
                ConsolePriority = Log.PriorityLevels.Medium,
                FilePriority = Log.PriorityLevels.UltraLow,
                DebuggerPriority = Log.PriorityLevels.UltraLow
            };

            if (!EnableLogging) { Log.Disable(); }
        }

        public async Task<Tuple<int, Edge, UpdateTime, EdgeUpdate>> GetUpdate(int updateCount, Edge edge, UpdateTime updateTime)
        {
            return await Task.Run(() =>
            {
                var update = new EdgeUpdate
                {
                    EdgeId = edge.Id,
                    UpdateHour = updateTime.HourRunTime
                };

                var travelMode = TravelMode.Driving;
                var occurrence = UpdateTime.GetNextOccurrence((int)updateTime.HourRunTime);
                occurrence = new DateTime(occurrence.Year, occurrence.Month, occurrence.Day, occurrence.Hour, 0, 0);

                Log.AddToLog(new LogMessage($"Project #{ Number }: Requesting edge '{edge.Id}' { travelMode.ToString().ToLower() } duration at { occurrence }.", Log.PriorityLevels.UltraLow));

                var xOrigin = edge.XFromPoint.ToString(CultureInfo.InvariantCulture);
                var yOrigin = edge.YFromPoint.ToString(CultureInfo.InvariantCulture);
                var xDestination = edge.XToPoint.ToString(CultureInfo.InvariantCulture);
                var yDestination = edge.YToPoint.ToString(CultureInfo.InvariantCulture);

                var directionsRequest = new DirectionsRequest
                {
                    Origin = $"{xOrigin},{yOrigin}",
                    Destination = $"{xDestination},{yDestination}",
                    DepartureTime = occurrence,
                    TravelMode = travelMode,
                    ApiKey = ApiKey
                };

                var requestTime = DateTime.Now;
                var response = GoogleMaps.Directions.Query(directionsRequest);

                // Log result.
                Log.AddToLog((response.Status == DirectionsStatusCodes.OK)
                    ? new LogMessage($"Project #{ Number }: Edge '{ edge.Id }' { travelMode.ToString().ToLower() } duration at { occurrence } response: { response.Status }.", Log.PriorityLevels.UltraLow)
                    : new LogMessage($"Project #{ Number }: Edge '{ edge.Id }' { travelMode.ToString().ToLower() } duration at { occurrence } response: { response.Status }.{(!string.IsNullOrEmpty(response.ErrorMessage) ? Environment.NewLine + "Error: " + response.ErrorMessage + "." : string.Empty)}", Log.PriorityLevels.Low));

                update.DepartureTime = occurrence;
                update.GoogleRequestTime = requestTime;
                update.GoogleStatus = response.Status;
                update.GoogleTravelMode = travelMode;
                update.GoogleErrorMessage = response?.ErrorMessage;
                update.GoogleDuration = response.Routes?.FirstOrDefault()?.Legs?.FirstOrDefault()?.DurationInTraffic?.Value;

                return new Tuple<int, Edge, UpdateTime, EdgeUpdate>(updateCount, edge, updateTime, update);
            });
        }

        // DONE: Set departure time!
        // DONE: Take oneway streets into consideration.
        // DONE: Add a timer.
        // TO DO: Test empty queue.
        public async Task<int> GetUpdates(ConcurrentQueue<Tuple<int, Edge, UpdateTime>> edges, List<UpdateTime> updateTimes)
        {
            if (MaxBatchRequests > MaxRequests) { return -2; }

            var requestCompletedCount = 0;
            var batchNumber = 0;
            var stopwatch = new Stopwatch();

            try
            {
                while (true)
                {
                    batchNumber++;

                    Log.AddToLog(new LogMessage($"Project #{ Number }: Loading batch #{ batchNumber }.", Log.PriorityLevels.Low));

                    var tasks = new List<Task<Tuple<int, Edge, UpdateTime, EdgeUpdate>>>();
                    var batchRequestCount = 0;

                    for (; batchRequestCount < MaxBatchRequests && ((requestCompletedCount + (batchRequestCount % MaxBatchRequests)) < MaxRequests); batchRequestCount++)
                    {
                        stopwatch.Restart();

                        if (!edges.TryDequeue(out Tuple<int, Edge, UpdateTime> currentEdge))
                        {
                            break;
                        }

                        tasks.Add(GetUpdate(currentEdge.Item1, currentEdge.Item2, currentEdge.Item3));
                    }

                    Log.AddToLog(new LogMessage($"Project #{ Number }: Loaded batch #{ batchNumber } with { batchRequestCount } edges.", Log.PriorityLevels.Low));

                    await Task.WhenAll(tasks);

                    tasks.Select(t => t.Result)
                        .ToList().ForEach(v =>
                        {
                            v.Item2.UpdateAndRequeue(v.Item1, v.Item4, updateTimes, edges);
                        });

                    // Summarise batch.
                    var batchSummary = new BatchSummary(batchNumber, tasks.Where(t => t.Status == TaskStatus.RanToCompletion).Select(t => t.Result).ToList());

                    requestCompletedCount += batchRequestCount;

                    Log.AddToLog(new LogMessage($"Project #{ Number }: Summary for batch #{ batchSummary.Number }.{ Environment.NewLine }{ batchSummary }", Log.PriorityLevels.Low));

                    Summary.Update(batchSummary);

                    // Actual request & response time is earlier but may as well include processing time required to validate, write to object and generate or update summaries.
                    stopwatch.Stop();

                    Log.AddToLog(new LogMessage($"Project #{ Number }: Batch #{ batchNumber } took { stopwatch.ElapsedMilliseconds } milliseconds to process.", Log.PriorityLevels.Medium));

                    if (requestCompletedCount >= MaxRequests)
                    {
                        break;
                    }

                    // This should never happen as we're currently requeuing.
                    if (edges.IsEmpty)
                    {
                        Log.AddToLog(new LogMessage($"Project #{ Number } can no longer retrieve data: There are no edges left to process.", Log.PriorityLevels.High));

                        break;
                    }

                    if (batchSummary.GoogleOverQueryLimitCount + batchSummary.GoogleRequestDeniedCount > 0)
                    {
                        Log.AddToLog(new LogMessage($"Project #{ Number } can no longer retrieve data: Quittable edge found (i.e., OVER_QUERY_LIMIT or REQUEST_DENIED).", Log.PriorityLevels.High));

                        break;
                    }

                    if (stopwatch.ElapsedMilliseconds < BatchIntervalTime)
                    {
                        var timeToWait = (int)(BatchIntervalTime - stopwatch.ElapsedMilliseconds);

                        Log.AddToLog(new LogMessage($"Project #{ Number }: Waiting { timeToWait } milliseconds.", Log.PriorityLevels.Medium));

                        Thread.Sleep(timeToWait);
                    }
                }

                Log.AddToLog(new LogMessage($"Project #{ Number } completed.{ Environment.NewLine }{ Summary }", Log.PriorityLevels.High));

                return 0;
            }
            catch (Exception e)
            {
                Log.AddToLog(new LogMessage($"Project #{ Number } has encountered an unexpected error. Project #{ Number } is aborting. Please ensure you are connected to the internet.", Log.PriorityLevels.UltraHigh));

                return -1;
            }
        }

        public static List<Project> GenerateTestProjects()
        {
            return  new List<Project>
            {
                new Project { Number = 1, ApiKey = "AIzaSyD_EFI7UTnUSKJk_R8_66tDD0_XHEujQVc" },
                new Project { Number = 2, ApiKey = "AIzaSyCAJzU9R8Y8UgtD1QoUHswUgRjnLMA7VJ4" },
                new Project { Number = 3, ApiKey = "AIzaSyCtoG6JK_SAu_On2rW4fZ_Wypp3K-xZ1WI" }
            };
        }
    }
}
