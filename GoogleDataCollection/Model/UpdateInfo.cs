using System;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using Newtonsoft.Json;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateInfo
    {
        [JsonProperty(PropertyName = "requestTime", Required = Required.Always)]
        public DateTime GoogleRequestTime { get; set; }

        [JsonProperty(PropertyName = "travelMode", Required = Required.Always)]
        public TravelMode TravelMode { get; set; }

        [JsonProperty(PropertyName = "departureTime", Required = Required.Always)]
        public DateTime DepartureTime { get; set; }

        [JsonProperty(PropertyName = "status", Required = Required.Always)]
        public DirectionsStatusCodes GoogleStatus { get; set; }

        [JsonProperty(PropertyName = "duration", Required = Required.AllowNull)]
        public TimeSpan? Duration { get; set; }

        [JsonProperty(PropertyName = "error", Required = Required.Default)]
        public string GoogleErrorMessage { get; set; }


        // REFERENCE: https://developers.google.com/maps/documentation/directions/intro#StatusCodes
        public static bool HasValidResponseStatus(UpdateInfo updateInfo)
        {
            switch (updateInfo.GoogleStatus)
            {
                case DirectionsStatusCodes.OK:
                    return true;

                case DirectionsStatusCodes.NOT_FOUND:
                    return true;

                case DirectionsStatusCodes.ZERO_RESULTS:
                    return true;

                case DirectionsStatusCodes.MAX_WAYPOINTS_EXCEEDED:
                    return true;

                case DirectionsStatusCodes.INVALID_REQUEST:
                    return false;

                case DirectionsStatusCodes.OVER_QUERY_LIMIT:
                    return false;

                case DirectionsStatusCodes.REQUEST_DENIED:
                    return false;

                case DirectionsStatusCodes.UNKNOWN_ERROR:
                    return true;

                default:
                    return false;
            }
        }
    }
}
