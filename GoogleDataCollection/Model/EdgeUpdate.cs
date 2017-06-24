using Newtonsoft.Json;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EdgeUpdate
    {
        [JsonProperty(PropertyName = "updateHour", Required = Required.Always)]
        public uint UpdateHour { get; set; }

        [JsonProperty(PropertyName = "forward", Required = Required.Always)]
        public UpdateInfo Forward { get; set; }

        [JsonProperty(PropertyName = "backward", Required = Required.AllowNull)]
        public UpdateInfo Backward { get; set; }
    }
}
