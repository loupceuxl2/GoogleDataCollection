using GoogleDataCollection.Model;
using Newtonsoft.Json;
using System.IO;

namespace GoogleDataCollection.DataAccess
{
    public static class JsonAccess
    {
        public static readonly string DefaultFilename = @"D:\Project4\Programming\C#\GoogleDataCollection\GoogleDataCollection\Data\output.json";

        public static PointToPointContainer DeserializePointToPoints()
        {
            PointToPointContainer container;

            using (StreamReader file = File.OpenText(DefaultFilename))
            {
                JsonSerializer serializer = new JsonSerializer();
                container = (PointToPointContainer)serializer.Deserialize(file, typeof(PointToPointContainer));
            }

            return container;
        }
    }
}
