using GoogleDataCollection.Logging;
using GoogleDataCollection.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// TO DO: DELETE!
// GDC1: AIzaSyD_EFI7UTnUSKJk_R8_66tDD0_XHEujQVc
// GDC2: AIzaSyCAJzU9R8Y8UgtD1QoUHswUgRjnLMA7VJ4
// GDC3: AIzaSyCtoG6JK_SAu_On2rW4fZ_Wypp3K-xZ1WI
namespace GoogleDataCollection.DataAccess
{
    public static class GoogleAccess
    {
        public static async Task<int> RunDataCollector(DataContainer data)
        {
            var updateSession = new UpdateSession();
            var executionSummary = new ExecutionSummary();
            var totalProjects = data.Projects.Count;

            // TO DO?: Remove duplicate projects (i.e., have the same HourRunTime) and order ascending (by HourRunTime).
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

            // TO DO: Filter out non drivable streets (requires 1. Changing 'HighwayType' to an enum; 2. Reparsing the data to reflect changes).
            // TO DO: Filter based on whether an edge contains updates which fail the IsRequeable test.
            // TO DO: Consider other filters based on data structure.
            var prioritisedUpdates =
                data.Edges.GroupBy(e => e.Updates.Count, e => e,
                        (key, g) => new {UpdateCount = key, Edges = g.ToList()                                                                                                          // Group edges by update count.
                        .OrderBy(edge => edge.Fid).ToList()})                                                                                                                           // Order individual groupings by FID.
                    .OrderBy(g => g.UpdateCount)                                                                                                                                        // Order groups by update count.
                    .Select(x => new { x.UpdateCount, x.Edges })                                                                                                                          // Transform anonymous type to new { int, List<Edge> }
                    .SelectMany(x => x.Edges.Select(y => new Tuple<int, Edge, UpdateTime>(x.UpdateCount, y, data.UpdateTimes[x.UpdateCount % data.UpdateTimes.Count])))                 // Flatten groups into a list of Tuples<int, Edge, UpdateTime>.
                    .ToList();

            var edgeQueue = new ConcurrentQueue<Tuple<int, Edge, UpdateTime>>(prioritisedUpdates);

            var totalEdges = edgeQueue.Count;

            Log.GlobalLog.AddToLog(new LogMessage($"{ edgeQueue.Count } edges loaded.", Log.PriorityLevels.Medium));

            if (totalEdges == 0)
            {
                Log.GlobalLog.AddToLog(new LogMessage($"No edges found. Data collection aborted.", Log.PriorityLevels.UltraHigh));

                return -1;
            }

            Log.GlobalLog.AddToLog(new LogMessage($"Data collection started.", Log.PriorityLevels.Medium));

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

            Log.GlobalLog.AddToLog(new LogMessage($"{ executionSummary }.", Log.PriorityLevels.High));

            updateSession.ExecutionSummary = executionSummary;
            updateSession.RunTimeCompletedAt = DateTime.Now;

            return 0;
        }
    }
}
