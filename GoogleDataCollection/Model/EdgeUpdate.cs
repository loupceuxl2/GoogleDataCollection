﻿using System;
using GoogleMapsApi.Entities.Directions.Response;
using Newtonsoft.Json;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class EdgeUpdate
    {
        [JsonProperty(PropertyName = "status", Required = Required.Always)]
        public DirectionsStatusCodes Status { get; set; }

        [JsonProperty(PropertyName = "updateTimeId", Required = Required.Always)]
        public Guid UpdateTimeId { get; set; }

        [JsonProperty(PropertyName = "duration", Required = Required.Default)]
        public TimeSpan Duration { get; set; }
    }
}
