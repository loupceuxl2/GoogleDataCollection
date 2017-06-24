﻿using GoogleMapsApi;
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
        public static uint MaxRequests = 200;

        public static uint MaxBatchRequests = 10;

        public static uint BatchIntervalTime = 1100;

        private static uint _numProjects;

        [JsonProperty(PropertyName = "apiKey", Required = Required.Always)]
        public string ApiKey { get; set; }

        public uint Number { get; set; }

        public UpdateSession CurrentSession { get; set; }

        public Log Log { get; set; }

        public Project()
        {
            Number = ++_numProjects;

            Log = new Log(new FileInfo($"{ AppDomain.CurrentDomain.BaseDirectory }\\project_{ Number }.txt"))
            {
                //Output = Log.OutputFormats.File | Log.OutputFormats.Console | Log.OutputFormats.Debugger,         // Add file output for distinct project logs.
                Output = Log.OutputFormats.Console,
                WriteMode = Log.WriteModes.Overwrite,
                ConsolePriority = Log.PriorityLevels.Medium,
                FilePriority = Log.PriorityLevels.UltraLow,
                DebuggerPriority = Log.PriorityLevels.UltraLow
            };
        }

        public async Task<Tuple<uint, Edge, UpdateSession.UpdateDirections, UpdateInfo, UpdateTime>> GetUpdate(Edge edge, UpdateSession.UpdateDirections direction, UpdateTime updateTime)
        {
            return await Task.Run(() =>
            {
                var updateInfo = new UpdateInfo();
                var travelMode = TravelMode.Driving;
                var occurrence = UpdateTime.GetNextOccurrence((int)updateTime.HourRunTime);
                occurrence = new DateTime(occurrence.Year, occurrence.Month, occurrence.Day, occurrence.Hour, 0, 0);

                Log.AddToLog(new LogMessage($"Project #{ Number }: Requesting edge {edge.Fid} { direction.ToString().ToLower() } { travelMode.ToString().ToLower() } traversal duration for { occurrence }.", Log.PriorityLevels.UltraLow));

                var xOrigin = direction == UpdateSession.UpdateDirections.Forwards ? edge.XFromPoint.ToString(CultureInfo.InvariantCulture) : edge.XToPoint.ToString(CultureInfo.InvariantCulture);
                var yOrigin = direction == UpdateSession.UpdateDirections.Forwards ? edge.YFromPoint.ToString(CultureInfo.InvariantCulture) : edge.YToPoint.ToString(CultureInfo.InvariantCulture);
                var xDestination = direction == UpdateSession.UpdateDirections.Forwards ? edge.XToPoint.ToString(CultureInfo.InvariantCulture) : edge.XFromPoint.ToString(CultureInfo.InvariantCulture);
                var yDestination = direction == UpdateSession.UpdateDirections.Forwards ? edge.YToPoint.ToString(CultureInfo.InvariantCulture) : edge.YFromPoint.ToString(CultureInfo.InvariantCulture);

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
                    : new LogMessage($"Project #{ Number }: Edge {edge.Fid} { direction.ToString().ToLower() } { travelMode.ToString().ToLower() } traversal duration for { occurrence } response: { response.Status }.{(!string.IsNullOrEmpty(response.ErrorMessage) ? Environment.NewLine + "Error: " + response.ErrorMessage + "." : string.Empty)}", Log.PriorityLevels.Medium));

                updateInfo.GoogleRequestTime = requestTime;
                updateInfo.GoogleStatus = response.Status;
                updateInfo.DepartureTime = occurrence;
                updateInfo.TravelMode = travelMode;
                updateInfo.GoogleErrorMessage = response?.ErrorMessage;
                updateInfo.Duration = response.Routes?.FirstOrDefault()?.Legs?.FirstOrDefault()?.DurationInTraffic?.Value;

                return new Tuple<uint, Edge, UpdateSession.UpdateDirections, UpdateInfo, UpdateTime>(edge.Fid, edge, direction, updateInfo, updateTime);
            });
        }

        // DONE: Set departure time!
        // DONE: Take oneway streets into consideration.
        // DONE: Add a timer.
        // TO DO: Take non streets into account (i.e., ignore them).
        // TO DO: Test empty queue.
        public async Task<int> GetUpdates(ConcurrentQueue<Tuple<int, Edge, UpdateTime>> edges)
        {
            var requestTotal = 0;
            var batchTotal = 0;
            var stopwatch = new Stopwatch();

            while (requestTotal < MaxRequests)
            {
                var batchNumber = 0;
                var tasks = new List<Task<Tuple<uint, Edge, UpdateSession.UpdateDirections, UpdateInfo, UpdateTime>>>();

                while (batchNumber < MaxBatchRequests)
                {
                    stopwatch.Restart();

                    if (!edges.TryDequeue(out Tuple<int, Edge, UpdateTime> currentEdge))
                    {
                        break;
                    }

                    if (currentEdge.Item2.IsOneWay)
                    {
                        Log.AddToLog(new LogMessage($"Project #{ Number }: Attempting one way data retrieval #{ requestTotal + 1 } of { MaxRequests } (#{ batchNumber + 1 } of batch #{ batchTotal + 1 }).", Log.PriorityLevels.UltraLow));

                        tasks.Add(GetUpdate(currentEdge.Item2, UpdateSession.UpdateDirections.Forwards, currentEdge.Item3));

                        requestTotal++;
                        batchNumber++;
                    }
                    else
                    {
                        //if (batchNumber + 1 >= MaxBatchRequests)
                        if (batchNumber + 2 > MaxBatchRequests)
                        {
                            Log.AddToLog(new LogMessage($"Project #{ Number }: Skipping two way data retrieval #{ requestTotal + 1 } of { MaxRequests } (#{ batchNumber + 1 } of batch #{ batchTotal + 1 }) AND #{ requestTotal + 2 } of { MaxRequests } (#{ batchNumber + 2 } of batch #{ batchTotal + 1 }).", Log.PriorityLevels.Low));

                            edges.Enqueue(currentEdge);

                            break;
                        }

                        Log.AddToLog(new LogMessage($"Project #{ Number }: Attempting two way data retrieval #{ requestTotal + 1 } of { MaxRequests } (#{ batchNumber + 1 } of batch #{ batchTotal + 1 }) AND #{ requestTotal + 2 } of { MaxRequests } (#{ batchNumber + 2 } of batch #{ batchTotal + 1 }).", Log.PriorityLevels.UltraLow));

                        tasks.Add(GetUpdate(currentEdge.Item2, UpdateSession.UpdateDirections.Forwards, currentEdge.Item3));
                        tasks.Add(GetUpdate(currentEdge.Item2, UpdateSession.UpdateDirections.Backwards, currentEdge.Item3));

                        requestTotal += 2;
                        batchNumber += 2;

                        // Lol, why not? Let's end once all requests have been exhausted.
                        edges.Enqueue(currentEdge);
                    }
                }

                batchTotal += 1;

                await Task.WhenAll(tasks);

                var summary = new BatchSummary(batchTotal, tasks);

                Log.AddToLog(new LogMessage($"Project #{ Number }{ Environment.NewLine }{ summary }.", Log.PriorityLevels.Medium));

                // Validate and write to object.

                // Actual time is earlier but may as well include processing time for generating summaries, validation and writing to object).
                stopwatch.Stop();

                Log.AddToLog(new LogMessage($"Project #{ Number }: Batch # { batchTotal } took { stopwatch.ElapsedMilliseconds } milliseconds.", Log.PriorityLevels.Medium));

                if (edges.IsEmpty)
                {
                    Log.AddToLog(new LogMessage($"Project #{ Number } completed.", Log.PriorityLevels.High));

                    // TO DO: Add project summary.

                    return 0;
                }

                if (stopwatch.ElapsedMilliseconds < BatchIntervalTime)
                {
                    var timeToWait = (int)(BatchIntervalTime - stopwatch.ElapsedMilliseconds);

                    Log.AddToLog(new LogMessage($"Project #{ Number }: Waiting { timeToWait } milliseconds.", Log.PriorityLevels.Medium));

                    Thread.Sleep(timeToWait);
                }
            }
            
            return 0;
        }
    }
}
