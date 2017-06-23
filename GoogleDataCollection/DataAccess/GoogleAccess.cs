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
        public static async Task<int> RunDataCollector(DataContainer data)
        {
            var totalProjects = data.Projects.Count;

            if (totalProjects == 0)
            {
                Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"No projects found. Data collection aborted.", Logging.Log.PriorityLevels.UltraHigh));

                return -1;
            }

            Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"{ totalProjects } projects loaded.", Logging.Log.PriorityLevels.Medium));

            var totalUpdateTimes = data.UpdateTimes.Count;

            if (totalUpdateTimes == 0)
            {
                Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"No update times found. Data collection aborted.", Logging.Log.PriorityLevels.UltraHigh));

                return -1;
            }

            Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"{ totalUpdateTimes } update times loaded.", Logging.Log.PriorityLevels.Medium));

            // TO DO: Check if there are no update times.
            var prioritisedUpdates =
                data.Edges.GroupBy(e => e.Updates.Count, e => e,
                        (key, g) => new {UpdateCount = key, Edges = g.ToList()                                                                                                          // Group edges by update count.
                        .OrderBy(edge => edge.Fid).ToList()})                                                                                                                           // Order individual groupings by FID.
                    .OrderBy(g => g.UpdateCount)                                                                                                                                        // Order groups by update count.
                    .Select(x => new {x.UpdateCount, x.Edges})                                                                                                                          // Transform anonymous type to new { int, List<Edge> }
                    .SelectMany(x => x.Edges.Select(y => new Tuple<int, Edge, UpdateTime>(x.UpdateCount, y, data.UpdateTimes[x.UpdateCount % data.UpdateTimes.Count])))                 // Flatten groups into a list of Tuples<int, Edge, UpdateTime>.
                    .ToList();

            var edgeQueue = new ConcurrentQueue<Tuple<int, Edge, UpdateTime>>(prioritisedUpdates);

            var totalEdges = edgeQueue.Count;

            Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"{ edgeQueue.Count } edges loaded.", Logging.Log.PriorityLevels.Medium));

            if (totalEdges == 0)
            {
                Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"No edges found. Data collection aborted.", Logging.Log.PriorityLevels.UltraHigh));

                return -1;
            }

            Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Data collection started.", Logging.Log.PriorityLevels.Medium));

            var tasks = new List<Task>(data.Projects
                .Select(p => Task.Run(() => p.GetUpdates(edgeQueue)))
                .ToList());

            await Task.WhenAll(tasks);

            Logging.Log.GlobalLog.AddToLog(new Logging.LogMessage($"Data collection completed successfully.", Logging.Log.PriorityLevels.Medium));

            // TO DO: Summary.

            return 0;
        }
    }
}
