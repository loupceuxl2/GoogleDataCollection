using GoogleDataCollection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleDataCollection.DataAccess
{
    public static class GoogleAccess
    {
        private static void InitialiseDataCollector(DataContainer data)
        {
            Console.WriteLine($"Loading projects...");
            Project.SetProjectUpdateSessions(data);
            Console.WriteLine($"Projects loaded (x{ data.Projects.Count }).{ Environment.NewLine }");
        }

        public static async Task<int> RunDataCollector(DataContainer data)
        {
            InitialiseDataCollector(data);

            var edges = DataContainer.EdgesToDictionary(data);
            var timeBrackets = data.TimeBrackets;

            // IMPORTANT: ToList() must be called!
            var tasks = new List<Task>(data.Projects
                .Select(p => Task.Run(() => p.GetUpdates(edges, timeBrackets)))
                .ToList());

            await Task.WhenAll(tasks);

            return 0;
        }
    }
}
