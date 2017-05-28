using GoogleDataCollection.DataAccess;
using GoogleDataCollection.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;

namespace GoogleDataCollection
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            //var container = SpreadsheetAccess.LoadData(SpreadsheetAccess.DefaultFilename, 1);

            //string output = JsonConvert.SerializeObject(container, Formatting.Indented);

            //File.WriteAllText(JsonAccess.DefaultFilename, output);
            //GoogleAccess.InitialiseDataCollector(JsonAccess.DeserializeEdges());

            var test = JsonAccess.DeserializeEdges();

            GoogleAccess.SetProjectUpdateSessions(test);


            //Console.WriteLine($"NEXT OCCURRENCE (14:00): { TimeBracket.GetNextOccurrence(13)} ");
            //Console.WriteLine($"UNIX TIMESTAMP (NOW): { TimeBracket.ConvertToUnixTimestamp(DateTime.Now) }");
            //Console.WriteLine($"UNIX TIMESTAMP (14:00): { TimeBracket.ConvertToUnixTimestamp(TimeBracket.GetNextOccurrence(13)) }");


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

/*
            foreach (var edge in test.Edges)
            {
                //Debug.WriteLine($"CURRENT EDGE FID: { edge.Fid }.");

                var progress = test.Edges.Count(e => 
                        e.XFromPoint.ToString(CultureInfo.InvariantCulture) == edge.XToPoint.ToString(CultureInfo.InvariantCulture)
                        && e.YFromPoint.ToString(CultureInfo.InvariantCulture) == edge.YToPoint.ToString(CultureInfo.InvariantCulture));

                if (progress > 2)
                {
                    Console.WriteLine($"CURRENT EDGE FID: { edge.Fid }.");
                    Console.WriteLine($"PROGRESS COUNT: {progress}.");
                }
            }
*/
