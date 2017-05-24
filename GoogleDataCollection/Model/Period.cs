using Newtonsoft.Json;

namespace GoogleDataCollection.Model
{
    public class Period
    {
        [JsonProperty(PropertyName = "name", Required = Required.Default)]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description", Required = Required.Default)]
        public string Description { get; set; }

        //String or DateTime?
        [JsonProperty(PropertyName = "startTime", Required = Required.Always)]
        public string StartTime { get; set; }

    }
}
