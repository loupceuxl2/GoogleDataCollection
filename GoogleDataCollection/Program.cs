using GoogleDataCollection.DataAccess;
using GoogleDataCollection.Model;
using Newtonsoft.Json;
using System;
using System.IO;

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

/*
            try
            {
                var data = JsonAccess.DeserializeEdges();

                Console.WriteLine($"{DateTime.Now}: Data collection started.");
                GoogleAccess.RunDataCollector(data).Wait();
                Console.WriteLine($"{DateTime.Now}: Data collection complete.");

                // DONE [!IMPORTANT]: Overwrite existing file (JsonAccess.DefaultFilename).
                File.WriteAllText($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ JsonAccess.DefaultFilename }", JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception");
                Console.WriteLine($"{e.Message}");
            }

*/

            // TO DO: Uncomment for release version.
/*
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
*/




            //const int hour = 9;

            //Console.WriteLine($"NEXT OCCURRENCE ({hour}:00): { TimeBracket.GetNextOccurrence(hour)} ");
            //Console.WriteLine($"UNIX TIMESTAMP ({hour}:00): { TimeBracket.ConvertToUnixTimestamp(TimeBracket.GetNextOccurrence(hour)) }");
        }
    }
}

/*
            Task<List<EdgeUpdate>[]> temp = null;
            Task.Run(() => temp = GoogleAccess.RunDataCollector(data)).Wait();

            foreach (var projects in temp.Result)
            {
                foreach (var projectUpdates in projects)
                {
                    Console.WriteLine($"DFS: {projectUpdates}");
                }
            }

*/

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
