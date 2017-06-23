using System.Collections.Generic;
using Newtonsoft.Json;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Edge
    {
        [JsonProperty(PropertyName = "fId", Required = Required.Always)]
        public uint Fid { get; set; }

        [JsonProperty(PropertyName = "osmId", Required = Required.Always)]
        public uint OsmId { get; set; }

        //52054
        [JsonProperty(PropertyName = "highwayName", Required = Required.AllowNull)]
        public string HighwayName { get; set; }

        // TO DO [OPTIONAL]: Change to an enum?
        [JsonProperty(PropertyName = "highwayType", Required = Required.Always)]
        public string HighwayType { get; set; }

        [JsonProperty(PropertyName = "isOneWay", Required = Required.Always)]
        public bool IsOneWay { get; set; }

        //[DefaultValue(0)]
        //[JsonProperty(PropertyName = "maxSpeed", Required = Required.Always, DefaultValueHandling = DefaultValueHandling.Populate)]
        [JsonProperty(PropertyName = "maxSpeed", Required = Required.AllowNull)]
        public uint? MaxSpeed { get; set; }

        [JsonProperty(PropertyName = "length", Required = Required.Always)]
        public double Length { get; set; }

        [JsonProperty(PropertyName = "xFromPoint", Required = Required.Always)]
        public double XFromPoint { get; set; }

        [JsonProperty(PropertyName = "yFromPoint", Required = Required.Always)]
        public double YFromPoint { get; set; }

        [JsonProperty(PropertyName = "xToPoint", Required = Required.Always)]
        public double XToPoint { get; set; }

        [JsonProperty(PropertyName = "yToPoint", Required = Required.Always)]
        public double YToPoint { get; set; }

        [JsonProperty(PropertyName = "xMidPoint", Required = Required.Always)]
        public double XMidPoint { get; set; }

        [JsonProperty(PropertyName = "yMidPoint", Required = Required.Always)]
        public double YMidPoint { get; set; }

        [JsonProperty(PropertyName = "updates", Required = Required.Always)]
        public List<EdgeUpdate> Updates { get; set; }

        public Edge()
        {
            Updates = new List<EdgeUpdate>();
        }
    }
}
