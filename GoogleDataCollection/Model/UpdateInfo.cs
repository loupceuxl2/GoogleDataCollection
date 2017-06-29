﻿using System;
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
        public TravelMode GoogleTravelMode { get; set; }

        [JsonProperty(PropertyName = "departureTime", Required = Required.Always)]
        public DateTime DepartureTime { get; set; }

        [JsonProperty(PropertyName = "status", Required = Required.Always)]
        public DirectionsStatusCodes GoogleStatus { get; set; }

        [JsonProperty(PropertyName = "duration", Required = Required.AllowNull)]
        public TimeSpan? GoogleDuration { get; set; } = null;

        [JsonProperty(PropertyName = "error", Required = Required.Default)]
        public string GoogleErrorMessage { get; set; }

        // REFERENCE: https://developers.google.com/maps/documentation/directions/intro#StatusCodes
        public static bool IsSavableUpdate(UpdateInfo updateInfo)
        {
            if (updateInfo == null) { return false; }

            switch (updateInfo.GoogleStatus)
            {
                case DirectionsStatusCodes.OK:
                    return true;

                case DirectionsStatusCodes.NOT_FOUND:
                    return false;

                case DirectionsStatusCodes.ZERO_RESULTS:
                    return false;

                // Should never occur for any versions of the program < 3.0.
                case DirectionsStatusCodes.MAX_WAYPOINTS_EXCEEDED:
                    return false;

                case DirectionsStatusCodes.INVALID_REQUEST:
                    return false;

                case DirectionsStatusCodes.OVER_QUERY_LIMIT:
                    return false;

                case DirectionsStatusCodes.REQUEST_DENIED:
                    return false;

                case DirectionsStatusCodes.UNKNOWN_ERROR:
                    return false;

                default:
                    return false;
            }
        }

        public static bool IsRequeuableUpdate(UpdateInfo updateInfo)
        {
            if (updateInfo == null) { return true; }

            switch (updateInfo.GoogleStatus)
            {
                case DirectionsStatusCodes.OK:
                    return true;

                case DirectionsStatusCodes.NOT_FOUND:
                    return false;

                case DirectionsStatusCodes.ZERO_RESULTS:
                    return false;

                case DirectionsStatusCodes.MAX_WAYPOINTS_EXCEEDED:
                    return true;

                case DirectionsStatusCodes.INVALID_REQUEST:
                    return true;

                case DirectionsStatusCodes.OVER_QUERY_LIMIT:
                    return true;

                case DirectionsStatusCodes.REQUEST_DENIED:
                    return true;

                case DirectionsStatusCodes.UNKNOWN_ERROR:
                    return true;

                default:
                    return true;
            }
        }

        public static bool IsQuittableUpdate(UpdateInfo updateInfo)
        {
            if (updateInfo == null) { return false; }

            switch (updateInfo.GoogleStatus)
            {
                case DirectionsStatusCodes.OK:
                    return false;

                case DirectionsStatusCodes.NOT_FOUND:
                    return false;

                case DirectionsStatusCodes.ZERO_RESULTS:
                    return false;

                case DirectionsStatusCodes.MAX_WAYPOINTS_EXCEEDED:
                    return false;

                case DirectionsStatusCodes.INVALID_REQUEST:
                    return false;

                case DirectionsStatusCodes.OVER_QUERY_LIMIT:
                    return true;

                case DirectionsStatusCodes.REQUEST_DENIED:
                    return true;

                case DirectionsStatusCodes.UNKNOWN_ERROR:
                    return false;

                default:
                    return false;
            }
        }
    }
}
