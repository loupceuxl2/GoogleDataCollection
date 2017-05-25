using System;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ErrorReport
    {
        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "time", Required = Required.Always)]
        public DateTime Time { get; set; }

        [JsonProperty(PropertyName = "errors", Required = Required.Always)]
        public List<PointToPointError> Errors { get; set; }
    }
}
