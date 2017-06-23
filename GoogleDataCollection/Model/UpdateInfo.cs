using System;
using GoogleMapsApi.Entities.Directions.Response;
using Newtonsoft.Json;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateInfo
    {
        [JsonProperty(PropertyName = "updatedAt", Required = Required.Always)]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty(PropertyName = "status", Required = Required.Always)]
        public DirectionsStatusCodes Status { get; set; }

        [JsonProperty(PropertyName = "duration", Required = Required.Default)]
        public TimeSpan Duration { get; set; }
    }
}
