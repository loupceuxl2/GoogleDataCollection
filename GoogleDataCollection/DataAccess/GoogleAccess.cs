using GoogleMapsApi;
using GoogleMapsApi.Entities.Directions.Request;
using GoogleMapsApi.Entities.Directions.Response;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GoogleDataCollection.Model;
using System.Threading.Tasks;

namespace GoogleDataCollection.DataAccess
{
    public static class GoogleAccess
    {
        // Make private?
        public static void InitialiseDataCollector(DataContainer data)
        {
            Console.WriteLine($"Loading projects...");
            Project.SetProjectUpdateSessions(data);
            Console.WriteLine($"Projects loaded (x{ data.Projects.Count }).{ Environment.NewLine }");
        }


        public async static Task<List<EdgeUpdate>[]> RunDataCollector(DataContainer data)
        {
            var edges = DataContainer.EdgesToDictionary(data);
            var timeBrackets = data.TimeBrackets;

            // IMPORTANT: ToList() must be called!
            //var tasks = new List<Task<EdgeUpdate>>();

            // IMPORTANT: ToList() must be called!
            var tasks = new List<Task<List<EdgeUpdate>>>(data.Projects
                .Select(p => Task.Run(() => p.GetUpdates(edges, timeBrackets)))
                .ToList());


            return await Task.WhenAll(tasks);



/*

            tasks.AddRange(parentSource == null
                ? childNodes.Select(child => (TaxonomyCreator) Activator.CreateInstance(ChildType, this, child))
                    .Select(childCreator => childCreator.Create())
                    .ToList()
                : childNodes.Select(child => (TaxonomyCreator) Activator.CreateInstance(ChildType, this, child, Source))
                    .Select(childCreator => childCreator.Create())
                    .ToList());

            return await Task.WhenAll(tasks);
*/
        }
    }
}

/*
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


            DirectionsResponse response = GoogleMaps.Directions.Query(directionsRequest);

            Console.Write(response.Routes.First().Legs.First().Duration);
            //File.WriteAllText("directions.txt", directions.Routes.ToString());
*/
