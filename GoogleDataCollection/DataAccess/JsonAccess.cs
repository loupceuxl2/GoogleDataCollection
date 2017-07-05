using System;
using System.IO;
using GoogleDataCollection.Logging;
using GoogleDataCollection.Model;
using Newtonsoft.Json;

namespace GoogleDataCollection.DataAccess
{
    public static class JsonAccess
    {
        public static readonly string DefaultFilename = "qld_network.json";

        private static DataContainer DeserializeEdges(string filename)
        {
            DataContainer container;

            if (!File.Exists($"{ filename }"))
            {
                Log.GlobalLog.AddToLog(new LogMessage($"JSON file '{ filename }' not found! Aborting operation.", Log.PriorityLevels.UltraHigh));

                return null;
            }

            using (var file = File.OpenText(filename))
            {
                container = (DataContainer)new JsonSerializer().Deserialize(file, typeof(DataContainer));
            }

            return container;
        }

        public static DataContainer DeserializeEdges()
        {
            return DeserializeEdges($@"{ AppDomain.CurrentDomain.BaseDirectory }\{ DefaultFilename }");
        }

        private static void SerializeEdges(DataContainer data, string filename)
        {
            Log.GlobalLog.AddToLog(new LogMessage($"Writing updates to file '{ filename }' started.", Log.PriorityLevels.Medium));
            File.WriteAllText($"{ filename }", JsonConvert.SerializeObject(data, Formatting.Indented));
            Log.GlobalLog.AddToLog(new LogMessage($"Writing updates to file '{ filename }' completed.", Log.PriorityLevels.Medium));
        }

        public static void SerializeEdges(DataContainer data)
        {
            SerializeEdges(data, $@"{ AppDomain.CurrentDomain.BaseDirectory }\{ DefaultFilename }");
        }
    }
}
