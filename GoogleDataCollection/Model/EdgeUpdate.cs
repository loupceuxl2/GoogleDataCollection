using Newtonsoft.Json;
using System;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EdgeUpdate
    {
        [JsonProperty(PropertyName = "updateTimeId", Required = Required.Always)]
        public Guid UpdateTimeId { get; set; }

        [JsonProperty(PropertyName = "forward", Required = Required.Always)]
        public UpdateInfo Forward { get; set; }

        [JsonProperty(PropertyName = "backward", Required = Required.AllowNull)]
        public UpdateInfo Backward { get; set; }
    }
}
