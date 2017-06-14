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
        private static void InitialiseDataCollector(DataContainer data)
        {
            Console.WriteLine($"Loading projects...");
            Project.SetProjectUpdateSessions(data);
            Console.WriteLine($"Projects loaded (x{ data.Projects.Count }).{ Environment.NewLine }");
        }

        public static async Task<int> RunDataCollector(DataContainer data)
        {
            InitialiseDataCollector(data);

            var edges = DataContainer.EdgesToDictionary(data);
            var timeBrackets = data.TimeBrackets;

            // IMPORTANT: ToList() must be called!
            var tasks = new List<Task>(data.Projects
                .Select(p => Task.Run(() => p.GetUpdates(edges, timeBrackets)))
                .ToList());

            await Task.WhenAll(tasks);

            Project.UpdateLastProjectSession(data);

            return 0;
        }

        public static async Task<int> RunDataCollector2(DataContainer data)
        {
            var prioritisedUpdates =
                data.Edges.GroupBy(e => e.Updates.Count, e => e,
                        (key, g) => new {UpdateCount = key, Edges = g.ToList()                                                                                                          // Group edges by update count.
                        .OrderBy(edge => edge.Fid).ToList()})                                                                                                                           // Order individual groupings by FID.
                    .OrderBy(g => g.UpdateCount)                                                                                                                                        // Order groups by update count.
                    //.OrderByDescending(g => g.UpdateCount)
                    .Select(x => new {x.UpdateCount, x.Edges})                                                                                                                          // Transform anonymous type to new { int, List<Edge> }
                    .SelectMany(x => x.Edges.Select(y => new Tuple<int, Edge, TimeBracket>(x.UpdateCount, y, data.TimeBrackets[x.UpdateCount % data.TimeBrackets.Count])))              // Flatten groups into a list of Tuples<int, Edge> | Tuples<UpdateCount, Edge>.
                    .ToList();

            var edgeQueue = new ConcurrentQueue<Tuple<int, Edge, TimeBracket>>(prioritisedUpdates);      
                         
            var tasks = new List<Task>(data.Projects
                .Select(p => Task.Run(() => p.GetUpdates(edgeQueue)))
                .ToList());

            await Task.WhenAll(tasks);

            return 0;
        }
    }
}
