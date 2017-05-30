using GoogleDataCollection.DataAccess;
using Newtonsoft.Json;
using System;
using System.IO;

namespace GoogleDataCollection
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var data = JsonAccess.DeserializeEdges();

                Console.WriteLine($"{DateTime.Now}: Data collection started.");
                GoogleAccess.RunDataCollector(data).Wait();
                Console.WriteLine($"{DateTime.Now}: Data collection complete.");

                // TO DO [!IMPORTANT]: Overwrite existing file (JsonAccess.DefaultFilename).
                File.WriteAllText($"{ AppDomain.CurrentDomain.BaseDirectory }\\new_file6.json", JsonConvert.SerializeObject(data, Formatting.Indented));
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception");
                Console.WriteLine($"{e.Message}");         
            }

            // TO DO: Uncomment for release versions.
/*
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
*/


            //var container = SpreadsheetAccess.LoadData(SpreadsheetAccess.DefaultFilename, 1);
            //string output = JsonConvert.SerializeObject(container, Formatting.Indented);
            //File.WriteAllText(JsonAccess.DefaultFilename, output);



            //Console.WriteLine($"NEXT OCCURRENCE (14:00): { TimeBracket.GetNextOccurrence(13)} ");
            //Console.WriteLine($"UNIX TIMESTAMP (NOW): { TimeBracket.ConvertToUnixTimestamp(DateTime.Now) }");
            //Console.WriteLine($"UNIX TIMESTAMP (14:00): { TimeBracket.ConvertToUnixTimestamp(TimeBracket.GetNextOccurrence(13)) }");
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
