﻿using GoogleMapsApi;
using GoogleMapsApi.Entities.Directions.Request;
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
using GoogleMapsApi.Entities.Directions.Response;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Project : ILog
    {
        public static uint MaxRequests = 200;

        public static uint MaxBatchRequests = 11;

        public static uint IntervalTime = 1500;

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
                Output = Log.OutputFormats.File | Log.OutputFormats.Console | Log.OutputFormats.Debugger,
                ConsolePriority = Log.PriorityLevels.UltraLow,
                FilePriority = Log.PriorityLevels.UltraLow,
                DebuggerPriority = Log.PriorityLevels.UltraLow
            };
        }

        // Return Task<Tuple<FID, Edge, DirectionsResponse>>
        public async Task<int> GetUpdate(Edge edge, UpdateSession.UpdateDirections direction, UpdateTime updateTime)
        {
            return await Task.Run(() =>
            {
                Log.AddToLog(new LogMessage($"Project #{ Number }: Edge {edge.Fid} data retrieval started.", Log.PriorityLevels.UltraLow));
                //Console.WriteLine($"Project #{ Number }: Edge {edge.Fid} data retrieval started.");
/*
                var response2 = new Tuple<uint, Edge, DirectionsResponse>(edge.Fid, edge, null);


                //Console.WriteLine($"Project #{ Number } | Edge #{ edge.Fid } data retrieval started... Seeking duration of travel for { occurrence }.");

                var update = new EdgeUpdate();

                var xOrigin = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.XFromPoint.ToString(CultureInfo.InvariantCulture) : edge.XToPoint.ToString(CultureInfo.InvariantCulture);
                var yOrigin = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.YFromPoint.ToString(CultureInfo.InvariantCulture) : edge.YToPoint.ToString(CultureInfo.InvariantCulture);
                var xDestination = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.XToPoint.ToString(CultureInfo.InvariantCulture) : edge.XFromPoint.ToString(CultureInfo.InvariantCulture);
                var yDestination = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.YToPoint.ToString(CultureInfo.InvariantCulture) : edge.YFromPoint.ToString(CultureInfo.InvariantCulture);

                var directionsRequest = new DirectionsRequest
                {
                    Origin = $"{xOrigin},{yOrigin}",
                    Destination = $"{xDestination},{yDestination}",
                    DepartureTime = occurrence,
                    TravelMode = TravelMode.Driving,
                    ApiKey = ApiKey
                };

                var response = GoogleMaps.Directions.Query(directionsRequest);

                update.Status = response.Status;
                update.UpdateTimeBracketId = session.CurrentTimeBracketId;

                //Console.WriteLine($"Project #{ Number } | Edge #{ edge.Fid } Google response status: {update.Status}");

                if (response.ErrorMessage != null)
                {
                    Console.WriteLine($"Project #{ Number } | Edge #{ edge.Fid } Google error message: { response.ErrorMessage }");
                }

                //var tempDuration = response.Routes?.FirstOrDefault()?.Legs?.FirstOrDefault()?.Duration;
                var tempDuration = response.Routes?.FirstOrDefault()?.Legs?.FirstOrDefault()?.DurationInTraffic;

                if (tempDuration == null)
                {
                    Console.WriteLine($"Project #{ Number } | Edge #{edge.Fid}: No information available.");
                    //return update;
                    return 0;
                }

                update.Duration = tempDuration.Value;

                //Console.WriteLine($"Project #{ Number } | Edge #{edge.Fid} duration: {update.Duration}.");
                //Console.WriteLine($"Project #{ Number } | Edge #{edge.Fid} data retrieval completed successfully.");

                //return update;
*/
                return 1;
            });
        }

        // DONE: Set departure time!
        // DONE: Take oneway streets into consideration.
        // TO DO: Take non streets into account (i.e., ignore them).
        // TO DO: Add a timer.
        // TO DO: Test empty queue.
        public async Task<int> GetUpdates(ConcurrentQueue<Tuple<int, Edge, UpdateTime>> edges)
        {
            var requestCount = 0;
            var processedCount = 0;

            while (requestCount < MaxRequests)
            {
                var batchCount = 0;
                var tasks = new List<Task>();

                while (batchCount < MaxBatchRequests)
                {
                    Tuple<int, Edge, UpdateTime> currentEdge;

                    if (!edges.TryDequeue(out currentEdge))
                    {
                        //Log.AddToLog(new LogMessage($"Project #{ Number } completed with x, y z", Log.PriorityLevels.Low));

                        break;
                    }

                    if (currentEdge.Item2.IsOneWay)
                    {
                        tasks.Add(GetUpdate(currentEdge.Item2, UpdateSession.UpdateDirections.Forwards, currentEdge.Item3));

                        batchCount++;
                    }
                    else
                    {
                        // TO DO: Test twoway streets.
                        //if (batchCount + 1 >= MaxBatchRequests)
                        if (batchCount + 2 > MaxBatchRequests)
                        {
                            Log.AddToLog(new LogMessage($"Project #{ Number }: Edge {currentEdge.Item2.Fid} data retrieval is being skipped.", Log.PriorityLevels.Low));

                            break;
                        }

                        tasks.Add(GetUpdate(currentEdge.Item2, UpdateSession.UpdateDirections.Forwards, currentEdge.Item3));
                        tasks.Add(GetUpdate(currentEdge.Item2, UpdateSession.UpdateDirections.Backwards, currentEdge.Item3));

                        batchCount += 2;
                    }
                }

                requestCount += batchCount;

                await Task.WhenAll(tasks);

                if (edges.IsEmpty)
                {

                    return 0;
                }

                // Validate and write to object.
            }
            
            return 0;
        }
    }
}

/*
public async Task<int> GetUpdates(Dictionary<uint, Edge> edges, List<TimeBracket> timeBrackets)
{
    return await Task.Run(() =>
    {
        var totalEdges = edges.Count;

        if (totalEdges == 0)
        {
            Console.WriteLine($"Project #{ Number } has { totalEdges } available.");
            return -1;
        }

        for (var i = 0; i < MaxRequests; i++)
        {
            if (CurrentSession == null)
            {
                Console.WriteLine($"Error [UpdateSession]: Project #{ Number } does not have an update session.");
                return -1;
            }

            var timeBracket =
                timeBrackets?.Find(tb => tb.Id.ToString() == CurrentSession.CurrentTimeBracketId.ToString());

            if (timeBracket == null)
            {
                Console.WriteLine($"Error [UpdateSession]: Project #{ Number } does not have a Time Bracket set.");
                return -1;
            }

            var occurrence = TimeBracket.GetNextOccurrence(timeBracket.HourRunTime);
            occurrence = new DateTime(occurrence.Year, occurrence.Month, occurrence.Day, occurrence.Hour, 0, 0);

            var edge = edges.ContainsKey(CurrentSession.CurrentEdgeFid) ? edges[CurrentSession.CurrentEdgeFid] : null;

            // TO DO: Add edge error handling data.
            if (edge == null)
            {
                Console.WriteLine($"Error [Edge]: Project #{ Number } |  Edge #{ CurrentSession.CurrentEdgeFid } not available.");
                CurrentSession = UpdateSession.GetNextUpdateSession((uint)totalEdges, CurrentSession, timeBrackets);
                continue;
            }

            if (CurrentSession.CurrentDirection == UpdateSession.UpdateDirections.Forwards)
            {
                edge.Updates.Add(GetUpdate(edge, CurrentSession, occurrence));
            }
            else
            {
                edge.InvertedUpdates.Add(GetUpdate(edge, CurrentSession, occurrence));
            }

            CurrentSession = UpdateSession.GetNextUpdateSession((uint)totalEdges, CurrentSession, timeBrackets);

            if ((i + 1) % MaxBatchRequests != 0) { continue; }

            Console.WriteLine($"Project #{ Number }: Maximum interval reached ({ MaxBatchRequests })," +
                              $" delaying ~{ IntervalTime / 1000 } seconds before starting a new batch." +
                              $" { i + 1 } of { MaxRequests } edges processed thus far.");
            Thread.Sleep(IntervalTime);
        }

        CurrentSession.RunTimeCompletedAt = DateTime.Now;

        return 0;
    });
}

public static void SetProjectUpdateSessions(DataContainer data)
{
    var totalEdges = (uint)data.Edges.Count;

    data.Projects[0].CurrentSession =
        data.UpdateSessions.LastOrDefault() ?? UpdateSession.GetNextUpdateSession(totalEdges, null, data.TimeBrackets);

    // !!! IF EdgeFid < previousId
    for (var i = 1; i < data.Projects.Count; i++)
    {
        var previousSession = data.Projects[i - 1].CurrentSession;
        var previousFid = previousSession.CurrentEdgeFid;
        var newFid = (MaxRequests + previousFid) % totalEdges;
        var direction = previousSession.CurrentDirection;
        var timeBracketId = previousSession.CurrentTimeBracketId;

        // TO DO [OPTIONAL]: Change below calls into two separate functions. Use in 'GetNextUpdateSession' method as well.
        if (newFid < previousFid)
        {
            direction = previousSession.CurrentDirection == UpdateSession.UpdateDirections.Forwards
                ? UpdateSession.UpdateDirections.Backwards
                : UpdateSession.UpdateDirections.Forwards;

            timeBracketId = previousSession.CurrentDirection == UpdateSession.UpdateDirections.Forwards
                ? previousSession.CurrentTimeBracketId
                : TimeBracket.GetNextTimeBracket(data.TimeBrackets, previousSession.CurrentTimeBracketId).Id;
        }

        var nextStartingPoint = new UpdateSession
        {
            CurrentEdgeFid = newFid,
            CurrentDirection = direction,
            CurrentTimeBracketId = timeBracketId
        };

        data.Projects[i].CurrentSession = nextStartingPoint;
    }

    // Set the project number.
    for (var i = 0; i < data.Projects.Count; i++)
    {
        var project = data.Projects[i];

        project.Number = i + 1;

        Console.WriteLine($"Project #{ project.Number } API key: { project.ApiKey }.");
        Console.WriteLine($"Project #{ project.Number } starting edge FID: { project.CurrentSession.CurrentEdgeFid }.");
        Console.WriteLine($"Project #{ project.Number } (hour) time bracket: { data.TimeBrackets.Single(b => b.Id.ToString() == project.CurrentSession.CurrentTimeBracketId.ToString()).HourRunTime }.");
        Console.WriteLine($"Project #{ project.Number } starting direction: { project.CurrentSession.CurrentDirection }.{ Environment.NewLine }");
    }
}

// DONE: Set departure time!
// TO DO: Take oneway streets into consideration (i.e., no need to invert).
// TO DO: Take non streets into account (i.e., ignore them).
public EdgeUpdate GetUpdate(Edge edge, UpdateSession session, DateTime occurrence)
{
    //Console.WriteLine($"Project #{ Number } | Edge #{ edge.Fid } data retrieval started... Seeking duration of travel for { occurrence }.");

    var update = new EdgeUpdate();

    var xOrigin = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.XFromPoint.ToString(CultureInfo.InvariantCulture) : edge.XToPoint.ToString(CultureInfo.InvariantCulture);
    var yOrigin = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.YFromPoint.ToString(CultureInfo.InvariantCulture) : edge.YToPoint.ToString(CultureInfo.InvariantCulture);
    var xDestination = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.XToPoint.ToString(CultureInfo.InvariantCulture) : edge.XFromPoint.ToString(CultureInfo.InvariantCulture);
    var yDestination = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.YToPoint.ToString(CultureInfo.InvariantCulture) : edge.YFromPoint.ToString(CultureInfo.InvariantCulture);

    var directionsRequest = new DirectionsRequest
    {
        Origin = $"{xOrigin},{yOrigin}",
        Destination = $"{xDestination},{yDestination}",
        DepartureTime = occurrence,
        TravelMode = TravelMode.Driving,
        ApiKey = ApiKey
    };

    var response = GoogleMaps.Directions.Query(directionsRequest);

    update.Status = response.Status;
    update.UpdateTimeBracketId = session.CurrentTimeBracketId;

    //Console.WriteLine($"Project #{ Number } | Edge #{ edge.Fid } Google response status: {update.Status}");

    if (response.ErrorMessage != null)
    {
        Console.WriteLine($"Project #{ Number } | Edge #{ edge.Fid } Google error message: { response.ErrorMessage }");
    }

    //var tempDuration = response.Routes?.FirstOrDefault()?.Legs?.FirstOrDefault()?.Duration;
    var tempDuration = response.Routes?.FirstOrDefault()?.Legs?.FirstOrDefault()?.DurationInTraffic;

    if (tempDuration == null)
    {
        Console.WriteLine($"Project #{ Number } | Edge #{edge.Fid}: No information available.");
        return update;
    }

    update.Duration = tempDuration.Value;

    //Console.WriteLine($"Project #{ Number } | Edge #{edge.Fid} duration: {update.Duration}.");
    //Console.WriteLine($"Project #{ Number } | Edge #{edge.Fid} data retrieval completed successfully.");

    return update;
}

public static void UpdateLastProjectSession(DataContainer data)
{
    var lastSession = data.Projects?.LastOrDefault()?.CurrentSession;

    if (lastSession == null) { return; }

    data.UpdateSessions.Add(lastSession);
}

*/
