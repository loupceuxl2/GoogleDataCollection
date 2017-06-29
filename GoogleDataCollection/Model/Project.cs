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
        public static uint MaxRequests = 2500;

        public static uint MaxBatchRequests = 50;

        public static uint BatchIntervalTime = 1100;

        private static uint _numProjects;

        [JsonProperty(PropertyName = "apiKey", Required = Required.Always)]
        public string ApiKey { get; set; }

        public uint Number { get; set; }

        public UpdateSession CurrentSession { get; set; }

        public ProjectSummary Summary { get; protected set; }

        public Log Log { get; set; }

        public Project()
        {
            Number = ++_numProjects;

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

        public async Task<Tuple<uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>> GetUpdate(Edge edge, EdgeUpdate.UpdateDirections direction, UpdateTime updateTime)
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

                return new Tuple<uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>(edge.Fid, edge, direction, updateInfo, updateTime);
            });
        }

        // DONE: Set departure time!
        // DONE: Take oneway streets into consideration.
        // DONE: Add a timer.
        // TO DO: Test empty queue.
        public async Task<int> GetUpdates(ConcurrentQueue<Tuple<int, Edge, UpdateTime>> edges, List<UpdateTime> updateTimes)
        {
            var requestTotal = 0;
            var batchTotal = 0;
            var stopwatch = new Stopwatch();

            try
            {
                while (requestTotal < MaxRequests)
                {
                    var batchNumber = 0;
                    var tasks = new List<Task<Tuple<uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>>>();

                    while (batchNumber < MaxBatchRequests)
                    {
                        stopwatch.Restart();

                        if (!edges.TryDequeue(out Tuple<int, Edge, UpdateTime> currentEdge))
                        {
                            break;
                        }

                        if (currentEdge.Item2.IsOneWay)
                        {
                            Log.AddToLog(new LogMessage($"Project #{ Number }: Attempting one way data retrieval #{ requestTotal + 1 } of { MaxRequests } (#{ batchNumber + 1 } of batch #{ batchTotal + 1 }) (Edge #{ currentEdge.Item2.Fid }).", Log.PriorityLevels.UltraLow));

                            tasks.Add(GetUpdate(currentEdge.Item2, EdgeUpdate.UpdateDirections.Forwards, currentEdge.Item3));

                            requestTotal++;
                            batchNumber++;
                        }
                        else
                        {
                            //if (batchNumber + 1 >= MaxBatchRequests)
                            if (batchNumber + 2 > MaxBatchRequests)
                            {
                                Log.AddToLog(new LogMessage($"Project #{ Number }: Skipping two way data retrieval #{ requestTotal + 1 } of { MaxRequests } (#{ batchNumber + 1 } of batch #{ batchTotal + 1 }) AND #{ requestTotal + 2 } of { MaxRequests } (#{ batchNumber + 2 } of batch #{ batchTotal + 1 }) (Edge #{ currentEdge.Item2.Fid }).", Log.PriorityLevels.Low));

                                // Requeue skipped item.
                                edges.Enqueue(currentEdge);

                                break;
                            }

                            Log.AddToLog(new LogMessage($"Project #{ Number }: Attempting two way data retrieval #{ requestTotal + 1 } of { MaxRequests } (#{ batchNumber + 1 } of batch #{ batchTotal + 1 }) AND #{ requestTotal + 2 } of { MaxRequests } (#{ batchNumber + 2 } of batch #{ batchTotal + 1 }) (Edge #{ currentEdge.Item2.Fid }).", Log.PriorityLevels.UltraLow));

                            tasks.Add(GetUpdate(currentEdge.Item2, EdgeUpdate.UpdateDirections.Forwards, currentEdge.Item3));
                            tasks.Add(GetUpdate(currentEdge.Item2, EdgeUpdate.UpdateDirections.Backwards, currentEdge.Item3));

                            requestTotal += 2;
                            batchNumber += 2;
                        }
                    }

                    batchTotal += 1;

                    await Task.WhenAll(tasks);

                    // Validate, write to object and requeue.
                    tasks.Where(t => t.Status == TaskStatus.RanToCompletion)
                        .Select(t => t.Result)
                        .GroupBy(t => t.Item2.Fid, t => t, (key, g) => new { edge = g.Where(v => v.Item2.Fid == key).Select(v => v.Item2).First(), updates = g.ToList() })
                        .Select(a => new { a.edge, a.updates })
                        .ToList().ForEach(v =>
                        {
                            v.edge.UpdateEdge(v.updates);
                            if (v.edge.IsRequeuable(v.updates)) { edges.Enqueue(new Tuple<int, Edge, UpdateTime>((int)v.updates[0].Item1 + 1, v.updates[0].Item2, updateTimes[((int)v.updates[0].Item1 + 1) % updateTimes.Count])); }
                        });

                    // Summarise batch.
                    var batchSummary = new BatchSummary(batchTotal, tasks);

                    Log.AddToLog(new LogMessage($"Project #{ Number }: Summary for batch #{ batchSummary.Number }.{ Environment.NewLine }{ batchSummary }.", Log.PriorityLevels.Low));

                    Summary.Update(batchSummary);

                    // Actual request & response time is earlier but may as well include processing time required to validate, write to object and generate or update summaries.
                    stopwatch.Stop();

                    Log.AddToLog(new LogMessage($"Project #{ Number }: Batch # { batchTotal } took { stopwatch.ElapsedMilliseconds } milliseconds to process.", Log.PriorityLevels.Medium));

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
    }
}

/*
                var toValidate = tasks.Where(t => t.Status == TaskStatus.RanToCompletion)
                    .Select(t => t.Result)
                    .GroupBy(t => t.Item2.Fid, t => t, (key, g) => new { edge = g.Where(v => v.Item2.Fid == key).Select(v => v.Item2).First(), updates = g.ToList() })
                    .Select(a => new { a.edge, a.updates })
                    .ToList();

                foreach (var result in toValidate)
                {
                    result.edge.UpdateEdge(result.updates);

                    if (result.edge.IsRequeuable(result.updates))
                    {
                        edges.Enqueue(new Tuple<int, Edge, UpdateTime>((int)result.updates[0].Item1 + 1, result.updates[0].Item2, updateTimes[((int)result.updates[0].Item1 + 1) % updateTimes.Count]));
                    }

                    if (result.edge.IsQuittable(result.updates))
                    {
                        isValid = false;
                    }
                }
*/
