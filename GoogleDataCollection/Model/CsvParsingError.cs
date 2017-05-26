using Newtonsoft.Json;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CsvParsingError
    {
        public enum ErrorTypes : byte { Unknown = 0, NullValue, InvalidColumn }

        [JsonProperty(PropertyName = "row", Required = Required.Always)]
        public uint Row { get; set; }

        [JsonProperty(PropertyName = "column", Required = Required.Always)]
        public uint Column { get; set; }

        [JsonProperty(PropertyName = "type", Required = Required.Always)]
        public ErrorTypes ErrorType { get; set; }

        [JsonProperty(PropertyName = "exceptionMessage", Required = Required.Default)]
        public string ExceptionMessage { get; set; }

        public CsvParsingError()
        {

        }
    }
}
