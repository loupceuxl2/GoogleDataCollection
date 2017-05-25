using Newtonsoft.Json;
using System.Collections.Generic;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PointToPointContainer
    {
        [JsonProperty(PropertyName = "pointToPoints", Required = Required.Always)]
        public List<PointToPoint> PointToPoints { get; set; }

        public PointToPointContainer()
        {
            PointToPoints = new List<PointToPoint>();
        }
/*
        public PointToPointContainer(int initCapacity = 100000)
        {
            PointToPoints = new List<PointToPoint>(initCapacity);
        }
*/
    }
}
