using Newtonsoft.Json;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class PointToPointError
    {
        public enum ErrorTypes : byte { Unknown = 0, NullValue, InvalidColumn }
        [JsonProperty(PropertyName = "row", Required = Required.Always)]
        public uint Row { get; set; }

        [JsonProperty(PropertyName = "column", Required = Required.Always)]
        public uint Column { get; set; }
    }
}
