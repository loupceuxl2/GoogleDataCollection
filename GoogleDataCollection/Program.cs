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
        // TO DO: Add note regarding deletion of TimeBracket will corrupt data.
        // TO DO: Add logging from Caliburn Micro app.
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

                Console.WriteLine($"{DateTime.Now}: Data collection started.");
                GoogleAccess.RunDataCollector(data).Wait();
                Console.WriteLine($"{DateTime.Now}: Data collection complete.");

/*
                Console.WriteLine($"{DateTime.Now}: Data collection started.");
                GoogleAccess.RunDataCollector(data).Wait();
                Console.WriteLine($"{DateTime.Now}: Data collection complete.");

                // DONE [!IMPORTANT]: Overwrite existing file (JsonAccess.DefaultFilename).
                File.WriteAllText($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ JsonAccess.DefaultFilename }", JsonConvert.SerializeObject(data, Formatting.Indented));
*/
/*
                var groupByUpdateCount4 =
                    data.Edges.GroupBy(e => e.Updates.Count, e => e,
                            (key, g) => new {UpdateCount = key, Edges = g.ToList()                                                                                                          // Group edges by update count.
                            .OrderBy(edge => edge.Fid).ToList()})                                                                                                                           // Order individual groupings by FID.
                        .OrderBy(g => g.UpdateCount)                                                                                                                                        // Order groups by update count.
                        //.OrderByDescending(g => g.UpdateCount)
                        //.ToList()
                        .Select(x => new {x.UpdateCount, x.Edges})                                                                                                                          // Transform anonymous type to new { int, List<Edge> }
                        .SelectMany(x => x.Edges.Select(y => new Tuple<int, Edge, TimeBracket>(x.UpdateCount, y, data.TimeBrackets[x.UpdateCount % data.TimeBrackets.Count])))              // Flatten groups into a list of Tuples<int, Edge> | Tuples<UpdateCount, Edge>.
                        .ToList();

                Console.WriteLine($"TUPLE COUNT: { groupByUpdateCount4.Count }.");
                
                //var oneWayCount = groupByUpdateCount4.Skip(50).Count(t => t.Item1 == 1);
                //Console.WriteLine($"TEST1 COUNT: { oneWayCount }.");

                var cq = new ConcurrentQueue<Tuple<int, Edge, TimeBracket>>(groupByUpdateCount4);
*/
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception");
                Console.WriteLine($"{e}");
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
