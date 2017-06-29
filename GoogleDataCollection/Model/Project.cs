using GoogleMapsApi;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GoogleDataCollection.Logging;

using System.Diagnostics;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Project : ILog
    {
        public static uint MaxRequests = 3;

        public static uint MaxBatchRequests = 2;

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
                Output = Log.OutputFormats.File | Log.OutputFormats.Console | Log.OutputFormats.Debugger,         // Add file output for distinct project logs.
                //Output = Log.OutputFormats.Console,
                WriteMode = Log.WriteModes.Overwrite,
                ConsolePriority = Log.PriorityLevels.Medium,
                FilePriority = Log.PriorityLevels.UltraLow,
                DebuggerPriority = Log.PriorityLevels.UltraLow
            };
        }

        public async Task<Tuple<int, uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>> GetUpdate(int updateCount, Edge edge, EdgeUpdate.UpdateDirections direction, UpdateTime updateTime)
        {
            return await Task.Run(() =>
            {
                var updateInfo = new UpdateInfo();
                var travelMode = TravelMode.Driving;
                var occurrence = UpdateTime.GetNextOccurrence((int)updateTime.HourRunTime);
                occurrence = new DateTime(occurrence.Year, occurrence.Month, occurrence.Day, occurrence.Hour, 0, 0);

                Log.AddToLog(new LogMessage($"Project #{ Number }: Requesting edge {edge.Fid} { direction.ToString().ToLower() } { travelMode.ToString().ToLower() } traversal duration for { occurrence }.", Log.PriorityLevels.UltraLow));

                var xOrigin = direction == EdgeUpdate.UpdateDirections.Forwards ? edge.XFromPoint.ToString(CultureInfo.InvariantCulture) : edge.XToPoint.ToString(CultureInfo.InvariantCulture);
                var yOrigin = direction == EdgeUpdate.UpdateDirections.Forwards ? edge.YFromPoint.ToString(CultureInfo.InvariantCulture) : edge.YToPoint.ToString(CultureInfo.InvariantCulture);
                var xDestination = direction == EdgeUpdate.UpdateDirections.Forwards ? edge.XToPoint.ToString(CultureInfo.InvariantCulture) : edge.XFromPoint.ToString(CultureInfo.InvariantCulture);
                var yDestination = direction == EdgeUpdate.UpdateDirections.Forwards ? edge.YToPoint.ToString(CultureInfo.InvariantCulture) : edge.YFromPoint.ToString(CultureInfo.InvariantCulture);

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

                Log.AddToLog((response.Status == DirectionsStatusCodes.OK)
                    ? new LogMessage($"Project #{ Number }: Edge {edge.Fid} { direction.ToString().ToLower() } { travelMode.ToString().ToLower() } traversal duration for { occurrence } response: { response.Status }.", Log.PriorityLevels.UltraLow)
                    : new LogMessage($"Project #{ Number }: Edge {edge.Fid} { direction.ToString().ToLower() } { travelMode.ToString().ToLower() } traversal duration for { occurrence } response: { response.Status }.{(!string.IsNullOrEmpty(response.ErrorMessage) ? Environment.NewLine + "Error: " + response.ErrorMessage + "." : string.Empty)}", Log.PriorityLevels.Low));

                updateInfo.DepartureTime = occurrence;
                updateInfo.GoogleRequestTime = requestTime;
                updateInfo.GoogleStatus = response.Status;
                updateInfo.GoogleTravelMode = travelMode;
                updateInfo.GoogleErrorMessage = response?.ErrorMessage;
                updateInfo.GoogleDuration = response.Routes?.FirstOrDefault()?.Legs?.FirstOrDefault()?.DurationInTraffic?.Value;

                return new Tuple<int, uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>(updateCount, edge.Fid, edge, direction, updateInfo, updateTime);
            });
        }

        // DONE: Set departure time!
        // DONE: Take oneway streets into consideration.
        // DONE: Add a timer.
        // TO DO: Test empty queue.
        public async Task<int> GetUpdates(ConcurrentQueue<Tuple<int, Edge, UpdateTime>> edges, List<UpdateTime> updateTimes)
        {
            var projectTotal = 0;
            var batchTotal = 0;
            var hasOverflowed = MaxBatchRequests < 2;
            var stopwatch = new Stopwatch();

            try
            {
                while (projectTotal < MaxRequests && (!hasOverflowed))
                {
                    var batchNumber = 0;
                    var tasks = new List<Task<Tuple<int, uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>>>();

                    while (batchNumber < MaxBatchRequests && projectTotal < MaxRequests)
                    {
                        stopwatch.Restart();

                        if (!edges.TryDequeue(out Tuple<int, Edge, UpdateTime> currentEdge))
                        {
                            break;
                        }

                        if (currentEdge.Item2.IsOneWay)
                        {
                            Log.AddToLog(new LogMessage($"Project #{ Number }: Attempting one way data retrieval #{ projectTotal + 1 } of { MaxRequests } (#{ batchNumber + 1 } of batch #{ batchTotal + 1 }) (Edge #{ currentEdge.Item2.Fid }).", Log.PriorityLevels.UltraLow));

                            tasks.Add(GetUpdate(currentEdge.Item1, currentEdge.Item2, EdgeUpdate.UpdateDirections.Forwards, currentEdge.Item3));

                            projectTotal++;
                            batchNumber++;
                        }
                        else
                        {
                            if (hasOverflowed = projectTotal + 2 > MaxRequests) { break; }

                            //if (batchNumber + 1 >= MaxBatchRequests)
                            if (batchNumber + 2 > MaxBatchRequests)
                            {
                                Log.AddToLog(new LogMessage($"Project #{ Number }: Skipping two way data retrieval #{ projectTotal + 1 } of { MaxRequests } (#{ batchNumber + 1 } of batch #{ batchTotal + 1 }) AND #{ projectTotal + 2 } of { MaxRequests } (#{ batchNumber + 2 } of batch #{ batchTotal + 1 }) (Edge #{ currentEdge.Item2.Fid }).", Log.PriorityLevels.Low));

                                // Requeue skipped item.
                                edges.Enqueue(currentEdge);

                                break;
                            }

                            Log.AddToLog(new LogMessage($"Project #{ Number }: Attempting two way data retrieval #{ projectTotal + 1 } of { MaxRequests } (#{ batchNumber + 1 } of batch #{ batchTotal + 1 }) AND #{ projectTotal + 2 } of { MaxRequests } (#{ batchNumber + 2 } of batch #{ batchTotal + 1 }) (Edge #{ currentEdge.Item2.Fid }).", Log.PriorityLevels.UltraLow));

                            tasks.Add(GetUpdate(currentEdge.Item1, currentEdge.Item2, EdgeUpdate.UpdateDirections.Forwards, currentEdge.Item3));
                            tasks.Add(GetUpdate(currentEdge.Item1, currentEdge.Item2, EdgeUpdate.UpdateDirections.Backwards, currentEdge.Item3));

                            projectTotal += 2;
                            batchNumber += 2;
                        }
                    }

                    await Task.WhenAll(tasks);

                    if (tasks.Count == 0) { break;  }

                    batchTotal += 1;

                    // Validate, write to object and requeue.
                    // DONE: Test requeuing is working correctly.
                    tasks.Where(t => t.Status == TaskStatus.RanToCompletion)
                        .Select(t => t.Result)
                        .GroupBy(t => t.Item3.Fid, t => t, (key, g) => new { edge = g.Where(v => v.Item3.Fid == key).Select(v => v.Item3).First(), updates = g.ToList() })
                        .Select(a => new { a.edge, a.updates })
                        .ToList().ForEach(v =>
                        {
                            v.edge.UpdateEdge(v.updates);
                            if (v.edge.IsRequeuable(v.updates)) { edges.Enqueue(new Tuple<int, Edge, UpdateTime>(v.updates[0].Item1 + 1, v.updates[0].Item3, updateTimes[(v.updates[0].Item1 + 1) % updateTimes.Count])); }
                        });

                    // Summarise batch.
                    var batchSummary = new BatchSummary(batchTotal, tasks);

                    Log.AddToLog(new LogMessage($"Project #{ Number }: Summary for batch #{ batchSummary.Number }.{ Environment.NewLine }{ batchSummary }", Log.PriorityLevels.Low));

                    Summary.Update(batchSummary);

                    // Actual request & response time is earlier but may as well include processing time required to validate, write to object and generate or update summaries.
                    stopwatch.Stop();

                    Log.AddToLog(new LogMessage($"Project #{ Number }: Batch #{ batchTotal } took { stopwatch.ElapsedMilliseconds } milliseconds to process.", Log.PriorityLevels.Medium));

                    // This will only happen if you somehow manage to get a TON of API keys :)
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
                Log.AddToLog(new LogMessage($"Project #{ Number } has encountered an unexpected error. Project #{ Number } is aborting. Please ensure you are connected to the internet before trying again.", Log.PriorityLevels.UltraHigh));

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
