using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateSession
    {
        public enum UpdateDirections : byte { Forwards, Backwards }

        [JsonProperty(PropertyName = "runTimeStartedAt", Required = Required.Always)]
        public DateTime RunTimeStartedAt { get; set; }

        [JsonProperty(PropertyName = "runTimeCompletedAt", Required = Required.Always)]
        public DateTime RunTimeCompletedAt { get; set; }

        [JsonProperty(PropertyName = "requestCount", Required = Required.Always)]
        public uint RequestCount { get; set; }

        // I.e., 
        [JsonProperty(PropertyName = "successCount", Required = Required.Always)]
        public uint SuccessCount { get; set; }

    }
}
