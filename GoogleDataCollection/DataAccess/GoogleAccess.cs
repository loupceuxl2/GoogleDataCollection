using GoogleMapsApi;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using System;
using System.IO;
using System.Linq;


namespace GoogleDataCollection.DataAccess
{
    public class GoogleAccess
    {
        public static void Test1()
        {
            DirectionsRequest directionsRequest = new DirectionsRequest()
            {
                Origin = "-27.9083023,153.4073545",
                Destination = "-27.9136192,153.4078265",
                TravelMode = TravelMode.Driving
                //SigningKey = "AIzaSyB6M2R9EZCnU5ZzKJGCy4AomaQr0iH1nJE"
            };

            var drivingDirectionsRequest = new DirectionsRequest
            {
                Origin = "NYC, 5th and 39",
                Destination = "Philladephia, Chesnut and Wallnut"
            };


            DirectionsResponse directions = GoogleMaps.Directions.Query(directionsRequest);

            Console.Write(directions.Routes.First().Legs.First().Duration);
            //File.WriteAllText("directions.txt", directions.Routes.ToString());
        }
    }
}
