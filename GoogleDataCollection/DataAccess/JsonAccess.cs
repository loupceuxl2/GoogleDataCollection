using System;
using System.IO;
using System.Linq;
using GoogleDataCollection.Logging;
using GoogleDataCollection.Model;
using Newtonsoft.Json;

namespace GoogleDataCollection.DataAccess
{
    public static class JsonAccess
    {
        public static readonly string DefaultFilename = "qld_network.json";

        public static DataContainer DeserializeEdges(string filename)
        {
            DataContainer container;

            using (var file = File.OpenText(filename))
            {
                var serializer = new JsonSerializer();
                container = (DataContainer)serializer.Deserialize(file, typeof(DataContainer));
            }

            return container;
        }

        public static DataContainer DeserializeEdges()
        {
            return DeserializeEdges($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ DefaultFilename }");
        }

        public static void SerializeEdges(DataContainer data)
        {
            Log.GlobalLog.AddToLog(new LogMessage($"Writing data to file '{ DefaultFilename }' started.", Log.PriorityLevels.Medium));
            File.WriteAllText($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ DefaultFilename }", JsonConvert.SerializeObject(data, Formatting.Indented));
            Log.GlobalLog.AddToLog(new LogMessage($"Writing data to file '{ DefaultFilename }' completed.", Log.PriorityLevels.Medium));
        }

        public static void CreateOverview(DataContainer data)
        {
            var overview = data.Edges
                .SelectMany(e => e.Updates)
                .GroupBy(u => u.UpdateHour)
                .Select(h => new { Hour = h.Key, Edges = data.Edges.Where(e => e.Updates.Count(u => u.UpdateHour == h.Key) > 0).Select(e => new { Edge = e, LatestDuration = e.Updates.Last(u => u.UpdateHour == h.Key).GoogleDuration } ) })
                ;



            Console.WriteLine($"OVERVIEW COUNT: { overview.Count() }.");

            foreach (var x in overview)
            {
                Console.WriteLine($"HOUR: { x.Hour }.");

                foreach (var y in x.Edges)
                {
                    Console.WriteLine($"\tID: { y.Edge.Id }.");
                    Console.WriteLine($"\tLATEST DURATION: { y.LatestDuration }.{ Environment.NewLine }");
                }

                Console.WriteLine($"{ Environment.NewLine } { Environment.NewLine } { Environment.NewLine } ");
            }
/*
            var prioritisedUpdates =
                data.Edges.Where(e => AcceptableHighwayTypes.Contains((Edge.HighwayTypes)e.HighwayType))                                                                                // Filter out non accepted highway types.
                    .GroupBy(e => e.Updates.Count, e => e,                                                                                                                              // Group edges by update count.
                        (key, g) => new {
                            UpdateCount = key,
                            Edges = g.ToList()
                        .OrderBy(edge => edge.Fid).ToList()
                        })                                                                                                                           // Order individual groupings by FID.
                    .OrderBy(g => g.UpdateCount)                                                                                                                                        // Order groups by update count.
                    .Select(x => new { x.UpdateCount, x.Edges })                                                                                                                          // Transform anonymous type to new { int, List<Edge> }
                    .SelectMany(x => x.Edges.Select(y => new Tuple<int, Edge, UpdateTime>(x.UpdateCount, y, data.UpdateTimes[x.UpdateCount % data.UpdateTimes.Count])))                 // Flatten groups into a list of Tuples<int, Edge, UpdateTime>.
                    .ToList();
*/
        }
    }
}
