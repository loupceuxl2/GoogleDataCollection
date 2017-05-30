using GoogleMapsApi;
using GoogleMapsApi.Entities.Directions.Request;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Project
    {
        // NOTE: MUST BE SMALLER THAN THE TOTAL NUMBER OF EDGES (LAST CHECKED 28/05/17: EDGES == 73738)
        //public static uint MaxRequests = 2500;
        public static uint MaxRequests = 20;

        //public static uint MaxIntervalRequests = 50;
        public static uint MaxIntervalRequests = 5;

        public static int IntervalTime = 1100;

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "apiKey", Required = Required.Always)]
        public string ApiKey { get; set; }

        public int Number { get; set; } = 0;

        public UpdateSession CurrentSession { get; set; }

        public Project()
        {
            Id = Guid.NewGuid();
        }

        // TO DO [!!!]: Set departure time!
        // TO DO: Take oneway streets into consideration (i.e., no need to invert).
        // TO DO: Take non streets into account (i.e., ignore them).
        public EdgeUpdate GetUpdate(Edge edge, UpdateSession session)
        {
            Console.WriteLine($"Project #{ Number } | Edge #{ edge.Fid } data retrieval started...");

            var update = new EdgeUpdate();

            var xOrigin = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.XFromPoint.ToString(CultureInfo.InvariantCulture) : edge.XToPoint.ToString(CultureInfo.InvariantCulture);
            var yOrigin = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.YFromPoint.ToString(CultureInfo.InvariantCulture) : edge.YToPoint.ToString(CultureInfo.InvariantCulture);
            var xDestination = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.XToPoint.ToString(CultureInfo.InvariantCulture) : edge.XFromPoint.ToString(CultureInfo.InvariantCulture);
            var yDestination = session.CurrentDirection == UpdateSession.UpdateDirections.Forwards ? edge.YToPoint.ToString(CultureInfo.InvariantCulture) : edge.YFromPoint.ToString(CultureInfo.InvariantCulture);

            var directionsRequest = new DirectionsRequest()
            {
                Origin = $"{xOrigin},{yOrigin}",
                Destination = $"{xDestination},{yDestination}",
                TravelMode = TravelMode.Driving,
                ApiKey = ApiKey
            };

            var response = GoogleMaps.Directions.Query(directionsRequest);

            update.Status = response.Status;
            update.UpdateTimeBracketId = session.CurrentTimeBracketId;

            Console.WriteLine($"Project #{ Number } | Edge #{ edge.Fid } Google response status: {update.Status}");

            var tempDuration = response.Routes?.FirstOrDefault()?.Legs?.FirstOrDefault()?.Duration;

            if (tempDuration == null)
            {
                Console.WriteLine($"Project #{ Number } | Edge #{edge.Fid}: No information available.");
                return update;
            }

            update.Duration = tempDuration.Value;

            Console.WriteLine($"Project #{ Number } | Edge #{edge.Fid} duration: {update.Duration}.");
            Console.WriteLine($"Project #{ Number } | Edge #{edge.Fid} data retrieval completed successfully.");

            return update;
        }

        public async Task<int> GetUpdates(Dictionary<uint, Edge> edges, List<TimeBracket> timeBrackets)
        {
            return await Task.Run(() =>
            {
                var totalEdges = edges.Count;

                if (CurrentSession == null)
                {
                    Console.WriteLine($"Error [UpdateSession]: Project #{ Number } does not have an update session.");
                    return -1;
                }

                for (var i = 0; i < MaxRequests; i++)
                {
                    var edge = edges.ContainsKey(CurrentSession.CurrentEdgeFid) ? edges[CurrentSession.CurrentEdgeFid] : null;

                    if (edge == null)
                    {
                        Console.WriteLine($"Error [Edge]: Project #{ Number } |  Edge #{ CurrentSession.CurrentEdgeFid } not available.");
                        CurrentSession = UpdateSession.GetNextUpdateSession((uint)totalEdges, CurrentSession, timeBrackets);
                        continue;
                    }

                    if (CurrentSession.CurrentDirection == UpdateSession.UpdateDirections.Forwards)
                    {
                        edge.Updates.Add(GetUpdate(edge, CurrentSession));
                    }
                    else
                    {
                        edge.InvertedUpdates.Add(GetUpdate(edge, CurrentSession));
                    }
                    
                    CurrentSession = UpdateSession.GetNextUpdateSession((uint)totalEdges, CurrentSession, timeBrackets);

                    if ((i + 1)%MaxIntervalRequests != 0) { continue; }

                    Console.WriteLine($"Project #{ Number }: Maximum interval reached ({ MaxIntervalRequests })," +
                                      $" delaying ~{ IntervalTime / 1000 } seconds before starting a new batch." +
                                      $" { i + 1 } of { MaxRequests } edges processed thus far.");
                    Thread.Sleep(IntervalTime);
                }

                CurrentSession.RunTimeCompleted = DateTime.Now;

                return 0;
            });
        }


        public static void SetProjectUpdateSessions(DataContainer data)
        {
            var totalEdges = (uint)data.Edges.Count;
/*
            var lastUpdateSession = new UpdateSession
            {
                EdgeFid = 73734,
                Direction = UpdateSession.UpdateDirections.Forwards,
                TimeBracketId = data.TimeBrackets.Last().Id
            };
*/
            //data.Projects[0].StartingPoint = UpdateSession.GetNextUpdateSession(totalEdges, lastUpdateSession, data.TimeBrackets);

            data.Projects[0].CurrentSession = 
                data.UpdateSessions.LastOrDefault() ?? UpdateSession.GetNextUpdateSession(totalEdges, null, data.TimeBrackets);
            //data.Projects[0].StartingPoint = UpdateSession.GetNextUpdateSession(totalEdges, null, data.TimeBrackets);

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

        public static void UpdateLastProjectSession(DataContainer data)
        {
            var lastSession = data.Projects?.LastOrDefault()?.CurrentSession;

            if (lastSession == null) { return; }

            data.UpdateSessions.Add(lastSession);
        }
    }
}
