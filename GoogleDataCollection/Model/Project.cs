using GoogleMapsApi;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Project
    {
        // NOTE: MUST BE SMALLER THAN THE TOTAL NUMBER OF EDGES (LAST CHECKED 28/05/17: EDGES == 73738)
        //public static uint MaxRequests = 2500;
        public static uint MaxRequests = 6;

        //public static uint MaxIntervalRequests = 50;
        public static uint MaxIntervalRequests = 2;


        public static uint IntervalTime = 1000;

        [JsonProperty(PropertyName = "id", Required = Required.Always)]
        public Guid Id { get; set; }

        [JsonProperty(PropertyName = "apiKey", Required = Required.Always)]
        public string ApiKey { get; set; }

        public UpdateSession StartingPoint { get; set; }

        public Project()
        {
            Id = Guid.NewGuid();
        }

        // TO DO: Set departure time!
        public EdgeUpdate GetUpdate(Edge edge, UpdateSession session)
        {
            if (StartingPoint == null)
            {
                return null;
            }

            var update = new EdgeUpdate();

            DirectionsRequest directionsRequest = new DirectionsRequest()
            {
                Origin = $"{edge.XFromPoint},{edge.YFromPoint}",
                Destination = $"{edge.XToPoint},{edge.YToPoint}",
                TravelMode = TravelMode.Driving,
                SigningKey = ApiKey
            };

            DirectionsResponse response = GoogleMaps.Directions.Query(directionsRequest);

            update.Status = response.Status;
            update.UpdateTimeBracketId = StartingPoint.TimeBracketId;

            if (response.Routes == null
                || response.Routes.FirstOrDefault() == null
                || response.Routes.First().Legs.FirstOrDefault() == null
                || response.Routes.First().Legs.First().Duration == null) { return update; }

            update.Duration = response.Routes.First().Legs.First().Duration.Value;

            return update;
        }

        public async Task<EdgeUpdate[]> GetUpdates(Dictionary<uint, Edge> edges)
        {
            var updates = new EdgeUpdate[0];

            if (StartingPoint == null)
            {
                return updates;
            }

            for (var i = 0; i < MaxRequests; i++)
            {
                if (i == IntervalTime)
                {
                    await Task.Delay(1000);
                }
            }

            return updates;
        }
    }
}
