using GoogleDataCollection.Model;
using Newtonsoft.Json;
using System.IO;

namespace GoogleDataCollection.DataAccess
{
    public static class JsonAccess
    {
        public static readonly string DefaultFilepath = @"D:\Project1\Programming\C#\GoogleDataCollection\GoogleDataCollection\Data\";
        public static readonly string DefaultFilename = "output.json";
        public static readonly string DefaultCompleteFilename = DefaultFilepath + DefaultFilename;

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
            return DeserializeEdges(DefaultCompleteFilename);
        }
    }
}
