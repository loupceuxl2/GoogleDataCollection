using GoogleDataCollection.DataAccess;
using GoogleDataCollection.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GoogleDataCollection
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            // !IMPORTANT: Uncomment to get a new (clean) Qld network JSON file. This will overwrite any existing "qld_network.json".
            // !IMPORTANT: Also, it will take a while (i.e., 2+ hours); I should not have used Interop.Excel.
/*
            var container = SpreadsheetAccess.LoadData(1);
            File.WriteAllText(JsonAccess.DefaultFilename, JsonConvert.SerializeObject(container, Formatting.Indented));
*/


            try
            {
                var data = JsonAccess.DeserializeEdges();

/*
                Console.WriteLine($"{DateTime.Now}: Data collection started.");
                GoogleAccess.RunDataCollector(data).Wait();
                Console.WriteLine($"{DateTime.Now}: Data collection complete.");

                // DONE [!IMPORTANT]: Overwrite existing file (JsonAccess.DefaultFilename).
                File.WriteAllText($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ JsonAccess.DefaultFilename }", JsonConvert.SerializeObject(data, Formatting.Indented));
*/

                //var groupByUpdateCount = data.Edges.GroupBy(e => e.Updates.Count).OrderBy(count => count.Key);
                var groupByUpdateCount1 = data.Edges.GroupBy(e => e.Updates.Count).ToList();
                var groupByUpdateCount2 = data.Edges.GroupBy(e => e.Updates.Count).OrderBy(count => count.Key).ToList();
                var groupByUpdateCount3 = data.Edges.GroupBy(e => e.Updates.Count, e => e, (key, g) => new { UpdateCount = key, Edges = g.ToList().OrderBy(edge => edge.Fid).ToList() }).OrderBy(g => g.UpdateCount).ToList();
                var groupByUpdateCount4 =
                    data.Edges.GroupBy(e => e.Updates.Count, e => e,
                            (key, g) => new {UpdateCount = key, Edges = g.ToList()                              // Group edges by update count.
                            .OrderBy(edge => edge.Fid).ToList()})                                               // Order individual groupings by FID.
                        .OrderBy(g => g.UpdateCount)                                                            // Order groups by update count.
                        .ToList()
                        .Select(x => new {x.UpdateCount, x.Edges})                                              // Transform anonymous type to new { int, List<Edge> }
                        .SelectMany(x => x.Edges.Select(y => new Tuple<int, Edge>(x.UpdateCount, y)))          // Flatten groups into a list of Tuples<int, Edge> | Tuples<UpdateCount, Edge>.
                        .ToList();
                //groupByUpdateCount2.ForEach(x => x.OrderBy(y => y.Fid));

                Console.WriteLine($"GROUPED COUNT: {groupByUpdateCount3.Count}.");
                //Console.WriteLine($"G1 KEY @ {0}: {groupByUpdateCount1[0].Key}.");
                //Console.WriteLine($"G2 KEY @ {0}: {groupByUpdateCount2[0].Key}.");
                Console.WriteLine($"G3 KEY @ {0}: {groupByUpdateCount3[0].UpdateCount}.");
                Console.WriteLine($"G3 KEY @ {0} FIRST FID: {groupByUpdateCount3[0].Edges.First().Fid}.");
                Console.WriteLine($"G3 KEY @ {0} LAST FID: {groupByUpdateCount3[0].Edges.Last().Fid}.");


                //groupByUpdateCount3.Select(x => new {x.UpdateCount, x.Edges}).SelectMany(y => y.Edges.Select(z => new Tuple<int, Edge>(y.UpdateCount, z)));
                var d = groupByUpdateCount3.Select(x => new { x.UpdateCount, x.Edges }).ToDictionary(y => y.UpdateCount, y => y.Edges);

                var result = d.SelectMany(kv => kv.Value.Select(s => new Tuple<int, Edge>(kv.Key, s))).ToList();

                Console.WriteLine($"TUPLE COUNT: {result.Count}.");
                Console.WriteLine($"TUPLE COUNT: {groupByUpdateCount4.Count()}.");
                //Dictionary<int, List<Edge>> d = groupByUpdateCount3.ToDictionary(ge => ge.UpdateCount);
                //var result = d.SelectMany(f => f.Key);

                //Console.WriteLine($"G3 KEY @ {0} LAST FID: {d[0].Edges.First().Fid}.");

                //var cq = new ConcurrentQueue<Edge>(groupByUpdateCount3.SelectMany(x => x.Edges));
                //Console.WriteLine($"CONCURRENT QUEUE COUNT: {cq.Count}.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception");
                Console.WriteLine($"{e.Message}");
            }



            // TO DO: Uncomment for release version.

            Console.WriteLine("Press enter to close...");
            Console.ReadLine();



            // EXAMPLE: Retrieve the next Time Bracket occurrence (based on a selected hour).
/*
            //const int hour = 9;

            //Console.WriteLine($"NEXT OCCURRENCE ({hour}:00): { TimeBracket.GetNextOccurrence(hour)} ");
            //Console.WriteLine($"UNIX TIMESTAMP ({hour}:00): { TimeBracket.ConvertToUnixTimestamp(TimeBracket.GetNextOccurrence(hour)) }");
*/
        }
    }
}

/*
            var test = JsonAccess.DeserializeEdges();


            foreach (var edge in test.Edges)
            {
                if (edge.MaxSpeed == 0)
                {
                    edge.MaxSpeed = null;
                }
            }

            var output2 = JsonConvert.SerializeObject(test, Formatting.Indented);

            File.WriteAllText(JsonAccess.DefaultFilepath + "new_file2.json", output2);

            var test2 = JsonAccess.DeserializeEdges(JsonAccess.DefaultFilepath + "new_file2.json");

            Console.WriteLine(test2.Edges.Count);
*/
