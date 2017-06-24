using GoogleMapsApi.Entities.Directions.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GoogleDataCollection.Logging;

namespace GoogleDataCollection.Model
{
    public class BatchSummary
    {
        public int Number { get; protected set; }

        public int TotalRequests { get; protected set; }
        public int GoogleOkCount { get; protected set; } = 0;
        public int GoogleNotOkCount { get; protected set; } = 0;
        public int DurationCount { get; protected set; } = 0;
        public int NullDurationCount { get; protected set; } = 0;
        public int GoogleErrorMessageCount { get; protected set; } = 0;


        public BatchSummary(int number, List<Task<Tuple<uint, Edge, UpdateSession.UpdateDirections, UpdateInfo, UpdateTime>>> results)
        {
            if (results == null)
            {
                return;
            }

            Number = number;

            TotalRequests = results.Count;
            GoogleOkCount = results.Count(t => t.Result.Item4.GoogleStatus == DirectionsStatusCodes.OK);
            GoogleNotOkCount = results.Count(t => t.Result.Item4.GoogleStatus != DirectionsStatusCodes.OK);
            DurationCount = results.Count(t => t.Result.Item4.Duration != null);
            NullDurationCount = results.Count(t => t.Result.Item4.Duration == null);
            GoogleErrorMessageCount = results.Count(t => !string.IsNullOrEmpty(t.Result.Item4.GoogleErrorMessage));
        }

        public override string ToString()
        {
            return $"===== BATCH #{ Number } SUMMARY ====={ Environment.NewLine }" +
                $"Total requests: { TotalRequests }{ Environment.NewLine }" +
                $"OK: { GoogleOkCount }{ Environment.NewLine }" +
                $"Not OK: { GoogleNotOkCount }{ Environment.NewLine }" +
                $"Has duration: { DurationCount }{ Environment.NewLine }" +
                $"No duration: { NullDurationCount }{Environment.NewLine}" +
                $"=============================";
        }
    }
}
