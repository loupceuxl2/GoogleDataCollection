using GoogleMapsApi;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoogleDataCollection.Model;


namespace GoogleDataCollection.DataAccess
{
    public static class GoogleAccess
    {
        public static void InitialiseProjects(DataContainer data)
        {
            //var lastUpdateSession = data.UpdateSessions.LastOrDefault();
            var totalEdges = data.Edges.Count;

            //data.Projects[0].StartingPoint = lastUpdateSession;
/*            
            
            var t = (uint)73400;

            for (var i = 1; i < data.Projects.Count; i++)
            {
                t = (uint) ((Project.MaxRequests * i - 1) % totalEdges);
                Console.WriteLine($"T: { t }.");
            }
*/
/*
            for (var i = 1; i < data.Projects.Count; i++)
            {
                var nextStartingPoint = new UpdateSession
                {
                    //!!!
                    EdgeFid = (uint)((Project.MaxRequests + data.Projects[i - 1].StartingPoint.EdgeFid) % totalEdges),
                    Direction = data.Projects[i - 1].StartingPoint.Direction,
                    TimeBracketId = data.Projects[i - 1].StartingPoint.TimeBracketId
                };

                data.Projects[i].StartingPoint = nextStartingPoint;
                //data.Projects[i].StartingPoint = GetNextUpdateSession((uint) totalEdges, nextStartingPoint, data.TimeBrackets);
            }
*/
/*
            for (var i = 1; i < data.Projects.Count; i++)
            {
                var nextStartingPoint = new UpdateSession
                {
                    //!!!
                    EdgeFid = (uint)((Project.MaxRequests + data.Projects[i - 1].StartingPoint.EdgeFid) % totalEdges),
                    Direction = data.Projects[i - 1].StartingPoint.Direction,
                    TimeBracketId = data.Projects[i - 1].StartingPoint.TimeBracketId
                };

                data.Projects[i].StartingPoint = nextStartingPoint;
                //data.Projects[i].StartingPoint = GetNextUpdateSession((uint) totalEdges, nextStartingPoint, data.TimeBrackets);
            }
*/

        } 

        public static void InitialiseDataCollector(DataContainer data, List<Project> projects)
        {
            var dictionary = DataContainer.EdgesToDictionary(data);
            var lastUpdateSession = data.UpdateSessions.LastOrDefault();
/*
            var lastUpdateSession = new UpdateSession
            {
                EdgeFid = 73734,
                Direction = UpdateSession.UpdateDirections.Backwards,
                TimeBracketId = data.TimeBrackets.First().Id
            };
*/
            var totalEdges = data.Edges.Count;
/*            
            var currentUpdateSession = GetNextUpdateSession((uint)totalEdges, lastUpdateSession, data.TimeBrackets);

            for (var i = 0; i < 20; i++)
            {
                var currentTimeBracket = data.TimeBrackets.Find(tb => tb.Id == currentUpdateSession.TimeBracketId);

                Console.WriteLine($"CURRENT FID: { currentUpdateSession.EdgeFid }");
                Console.WriteLine($"CURRENT TIME BRACKET: { currentTimeBracket.Name }");
                Console.WriteLine($"CURRENT DIRECTION: { currentUpdateSession.Direction }");

                currentUpdateSession = GetNextUpdateSession((uint)totalEdges, currentUpdateSession, data.TimeBrackets);
            }
*/            

            Console.WriteLine($"DICTIONARY COUNT: { dictionary.Count }");

/*
            DirectionsRequest directionsRequest = new DirectionsRequest()
            {
                Origin = "-27.9083023,153.4073545",
                Destination = "-27.9136192,153.4078265",
                TravelMode = TravelMode.Driving
                //SigningKey = "AIzaSyB6M2R9EZCnU5ZzKJGCy4AomaQr0iH1nJE"
            };

            var drivingDirectionsRequest = new DirectionsRequest
            {
                Origin = "NYC, 5th and 39",
                Destination = "Philladephia, Chesnut and Wallnut"
            };


            DirectionsResponse response = GoogleMaps.Directions.Query(directionsRequest);

            Console.Write(response.Routes.First().Legs.First().Duration);
            //File.WriteAllText("directions.txt", directions.Routes.ToString());
*/
        }


        public static void RunDataCollector(Dictionary<uint, Edge> dictionary, TimeBracket currentTimeBracket, uint currentFid, UpdateSession.UpdateDirections currentDirection)
        {
            
        }

        public static void SetProjectUpdateSessions(DataContainer data)
        {
            var totalEdges = (uint)data.Edges.Count;

            var lastUpdateSession = new UpdateSession
            {
                EdgeFid = 73734,
                Direction = UpdateSession.UpdateDirections.Forwards,
                TimeBracketId = data.TimeBrackets.Last().Id
            };

            data.Projects[0].StartingPoint = GetNextUpdateSession(totalEdges, lastUpdateSession, data.TimeBrackets);

            // BECAUSE WE'RE GOING +5.
            // !!! IF EdgeFid < previousId
            for (var i = 1; i < data.Projects.Count; i++)
            {
                var previousSession = data.Projects[i - 1].StartingPoint;
                var previousFid = previousSession.EdgeFid;
                var newFid = (Project.MaxRequests + previousFid) % totalEdges;
                var direction = previousSession.Direction;
                var timeBracketId = previousSession.TimeBracketId;

                // TO DO [OPTIONAL]: Change below calls into two separate functions. Use in 'GetNextUpdateSession' method as well.
                if (newFid < previousFid)
                {
                    direction = previousSession.Direction == UpdateSession.UpdateDirections.Forwards
                        ? UpdateSession.UpdateDirections.Backwards
                        : UpdateSession.UpdateDirections.Forwards;

                    timeBracketId = previousSession.Direction == UpdateSession.UpdateDirections.Forwards
                        ? previousSession.TimeBracketId
                        : GetNextTimeBracket(data.TimeBrackets, previousSession.TimeBracketId).Id;
                }
                

                var nextStartingPoint = new UpdateSession
                {
                    EdgeFid = newFid,
                    Direction = direction,
                    TimeBracketId = timeBracketId
                    //Direction = data.Projects[i - 1].StartingPoint.Direction,
                    //TimeBracketId = data.Projects[i - 1].StartingPoint.TimeBracketId
                };

                data.Projects[i].StartingPoint = nextStartingPoint;
                //data.Projects[i].StartingPoint = GetNextUpdateSession((uint) totalEdges, nextStartingPoint, data.TimeBrackets);
            }

            foreach (var project in data.Projects)
            {
                Console.WriteLine($"PROJECT STARTING FID: { project.StartingPoint.EdgeFid }.");
                Console.WriteLine($"PROJECT STARTING BRACKET: { project.StartingPoint.TimeBracketId }");
                Console.WriteLine($"PROJECT STARTING DIRECTION: { project.StartingPoint.Direction }");
            }
        }

        public static UpdateSession GetNextUpdateSession(uint totalFids, UpdateSession currentSession, List<TimeBracket> brackets)
        {
            // First time running (no previous sessions to continue from).
            if (currentSession == null)
            {
                return new UpdateSession
                {
                    EdgeFid = 0,
                    Direction = UpdateSession.UpdateDirections.Forwards,
                    TimeBracketId = brackets.First().Id
                };
            }

            // If we haven't reached the end of the collection go to the next Fid.
            if (currentSession.EdgeFid != totalFids - 1)
            {
                return new UpdateSession
                {
                    EdgeFid = currentSession.EdgeFid + 1,
                    Direction = currentSession.Direction,
                    TimeBracketId = currentSession.TimeBracketId
                };
            }

            // End of collection, decide where to go next.
            return new UpdateSession
            {
                EdgeFid = 0,
                Direction = currentSession.Direction == UpdateSession.UpdateDirections.Forwards ? UpdateSession.UpdateDirections.Backwards : UpdateSession.UpdateDirections.Forwards,
                TimeBracketId = (currentSession.Direction == UpdateSession.UpdateDirections.Forwards) ? currentSession.TimeBracketId : GetNextTimeBracket(brackets, currentSession.TimeBracketId).Id
            };
        }

        public static TimeBracket GetNextTimeBracket(List<TimeBracket> brackets, Guid currentTimeBracketId)
        {
            var indexOfCurrentTimeBracket = brackets.FindIndex(tb => tb.Id == currentTimeBracketId);
            var nextIndex = (indexOfCurrentTimeBracket % brackets.Count) + 1;

            return brackets[nextIndex < brackets.Count ? nextIndex : 0];
        }
    }
}
