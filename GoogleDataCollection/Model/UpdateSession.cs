using System;
using Newtonsoft.Json;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateSession
    {
        public enum UpdateDirections : byte { Forwards, Backwards }

        [JsonProperty(PropertyName = "lastRunTime", Required = Required.Always)]
        public DateTime RunTime { get; set; }

        [JsonProperty(PropertyName = "lastTimeBracketId", Required = Required.Always)]
        public Guid TimeBracketId { get; set; }

        [JsonProperty(PropertyName = "lastEdgeFid", Required = Required.Always)]
        public uint EdgeFid { get; set; }

        [JsonProperty(PropertyName = "lastDirection", Required = Required.Always)]
        public UpdateDirections Direction { get; set; }
    }
}
