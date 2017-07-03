using GoogleDataCollection.Logging;
using GoogleDataCollection.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleDataCollection.DataAccess
{
    public static class GoogleAccess
    {
        public static HashSet<Edge.HighwayTypes> AcceptableHighwayTypes = new HashSet<Edge.HighwayTypes>
        {
            Edge.HighwayTypes.Motorway,
            Edge.HighwayTypes.Trunk,
            Edge.HighwayTypes.Primary,
            Edge.HighwayTypes.Secondary,
            Edge.HighwayTypes.Tertiary,
            Edge.HighwayTypes.Unclassified,
            Edge.HighwayTypes.Residential,
            Edge.HighwayTypes.Service,
            Edge.HighwayTypes.MotorwayLink,
            Edge.HighwayTypes.TrunkLink,
            Edge.HighwayTypes.PrimaryLink,
            Edge.HighwayTypes.SecondaryLink,
            Edge.HighwayTypes.TertiaryLink,
            Edge.HighwayTypes.LivingStreet,
            Edge.HighwayTypes.Pedestrian,
            Edge.HighwayTypes.Track,
            Edge.HighwayTypes.BusGuideway,
            Edge.HighwayTypes.Escape,
            Edge.HighwayTypes.Raceway,
            Edge.HighwayTypes.Road,
            Edge.HighwayTypes.Private,
            Edge.HighwayTypes.Unsurfaced,
            Edge.HighwayTypes.Minor,
            Edge.HighwayTypes.Ford,
            Edge.HighwayTypes.ClosedTrunk,
            Edge.HighwayTypes.Unknown
        };

        public static async Task<int> RunDataCollector(DataContainer data)
        {
            var updateSession = new UpdateSession();
            var executionSummary = new ExecutionSummary();
            var totalProjects = data.Projects.Count;

            // TO DO [OPTIONAL]: Remove duplicate projects (i.e., have the same HourRunTime) and order ascending (by HourRunTime).
            if (totalProjects == 0)
            {
                Log.GlobalLog.AddToLog(new LogMessage($"No projects found. Data collection aborted.", Log.PriorityLevels.UltraHigh));

                return -1;
            }

            Log.GlobalLog.AddToLog(new LogMessage($"{ totalProjects } projects loaded.", Log.PriorityLevels.Medium));

            var totalUpdateTimes = data.UpdateTimes.Count;

            // DONE: Check if there are no update times.
            if (totalUpdateTimes == 0)
            {
                Log.GlobalLog.AddToLog(new LogMessage($"No update times found. Data collection aborted.", Log.PriorityLevels.UltraHigh));

                return -1;
            }

            Log.GlobalLog.AddToLog(new LogMessage($"{ totalUpdateTimes } update times loaded.", Log.PriorityLevels.Medium));

            // DONE: Filter out non drivable streets (requires 1. Changing 'HighwayType' to an enum; 2. Reparsing the data to reflect changes).
            // TO DO [OPTIONAL]: Consider other filters based on data structure.
            var prioritisedUpdates =
                data.Edges.Where(e => AcceptableHighwayTypes.Contains((Edge.HighwayTypes)e.HighwayType))                                                                                // Filter out non accepted highway types.
                    .GroupBy(e => e.Updates.Count, e => e,                                                                                                                              // Group edges by update count.
                        (key, g) => new {UpdateCount = key, Edges = g.ToList()                                                                                                          
                        .OrderBy(edge => edge.Fid).ToList()})                                                                                                                           // Order individual groupings by FID.
                    .OrderBy(g => g.UpdateCount)                                                                                                                                        // Order groups by update count.
                    .Select(x => new { x.UpdateCount, x.Edges })                                                                                                                          // Transform anonymous type to new { int, List<Edge> }
                    .SelectMany(x => x.Edges.Select(y => new Tuple<int, Edge, UpdateTime>(x.UpdateCount, y, data.UpdateTimes[x.UpdateCount % data.UpdateTimes.Count])))                 // Flatten groups into a list of Tuples<int, Edge, UpdateTime>.
                    .ToList();

            var edgeQueue = new ConcurrentQueue<Tuple<int, Edge, UpdateTime>>(prioritisedUpdates);

            Log.GlobalLog.AddToLog(new LogMessage($"{ edgeQueue.Count } edges loaded. { data.Edges.Count - edgeQueue.Count } filtered out (from an initial total of { data.Edges.Count }).", Log.PriorityLevels.Medium));

            if (edgeQueue.Count == 0)
            {
                Log.GlobalLog.AddToLog(new LogMessage($"No edges found. Data collection aborted.", Log.PriorityLevels.UltraHigh));

                return -1;
            }

            Log.GlobalLog.AddToLog(new LogMessage($"Data collection started.", Log.PriorityLevels.Medium));

            data.Projects.ForEach(p => p.LoadProject());

            data.UpdateSessions.Add(updateSession);
            updateSession.RunTimeStartedAt = DateTime.Now;
            updateSession.ExecutionSummary = executionSummary;

            var tasks = new List<Task>(data.Projects
                .Select(p => Task.Run(() => p.GetUpdates(edgeQueue, new List<UpdateTime>(data.UpdateTimes))))
                .ToList());

            await Task.WhenAll(tasks);

            Log.GlobalLog.AddToLog(new LogMessage($"Data collection completed successfully.", Log.PriorityLevels.Medium));

            // DONE: Overall summary.
            data.Projects.ForEach(p => executionSummary.Update(p.Summary));

            Log.GlobalLog.AddToLog(new LogMessage($"{ executionSummary }", Log.PriorityLevels.High));

            updateSession.ExecutionSummary = executionSummary;
            updateSession.RunTimeCompletedAt = DateTime.Now;

            return 0;
        }
    }
}
