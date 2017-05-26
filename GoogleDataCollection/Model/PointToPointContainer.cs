using Newtonsoft.Json;
using System.Collections.Generic;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PointToPointContainer
    {
        [JsonProperty(PropertyName = "pointToPoints", Required = Required.Always)]
        public List<PointToPoint> PointToPoints { get; set; }

        [JsonProperty(PropertyName = "csvParsing", Required = Required.Default)]
        public CsvParsing CsvParsing { get; set; }

        public PointToPointContainer()
        {
            PointToPoints = new List<PointToPoint>();
        }
    }
}
