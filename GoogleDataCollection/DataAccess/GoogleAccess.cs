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
            // TO DO: Check if there are no update times.
/*
            var log = new Log
            {
                Output = Log.OutputFormats.File | Log.OutputFormats.Console | Log.OutputFormats.Debugger,
                ConsolePriority = Log.PriorityLevels.UltraLow
            };

            data.Projects.ForEach(p => p.Log = log);
*/
            var prioritisedUpdates =
                data.Edges.GroupBy(e => e.Updates.Count, e => e,
                        (key, g) => new {UpdateCount = key, Edges = g.ToList()                                                                                                          // Group edges by update count.
                        .OrderBy(edge => edge.Fid).ToList()})                                                                                                                           // Order individual groupings by FID.
                    .OrderBy(g => g.UpdateCount)                                                                                                                                        // Order groups by update count.
                    //.OrderByDescending(g => g.UpdateCount)
                    .Select(x => new {x.UpdateCount, x.Edges})                                                                                                                          // Transform anonymous type to new { int, List<Edge> }
                    .SelectMany(x => x.Edges.Select(y => new Tuple<int, Edge, UpdateTime>(x.UpdateCount, y, data.UpdateTimes[x.UpdateCount % data.UpdateTimes.Count])))                 // Flatten groups into a list of Tuples<int, Edge, UpdateTime>.
                    .ToList();

            var edgeQueue = new ConcurrentQueue<Tuple<int, Edge, UpdateTime>>(prioritisedUpdates);

            var tasks = new List<Task>(data.Projects
                .Select(p => Task.Run(() => p.GetUpdates(edgeQueue)))
                .ToList());

            await Task.WhenAll(tasks);

            return 0;
        }
    }
}
