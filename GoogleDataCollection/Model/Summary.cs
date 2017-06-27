using GoogleMapsApi.Entities.Directions.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GoogleDataCollection.Model
{
    public abstract class Summary
    {
        public abstract string Name { get; protected set; }

        public int Number { get; protected set; }

        public int TotalRequests { get; protected set; } = 0;
        public int OneWayCount { get; protected set; } = 0;
        public int TwoWayCount { get; protected set; } = 0;
        public int GoogleOkCount { get; protected set; } = 0;
        public int GoogleNotOkCount { get; protected set; } = 0;
        public int GoogleNotFoundCount { get; protected set; } = 0;
        public int GoogleZeroResultsCount { get; protected set; } = 0;
        public int GoogleMaxWaypointsExceededCount { get; protected set; } = 0;
        public int GoogleInvalidRequestCount { get; protected set; } = 0;
        public int GoogleOverQueryLimitCount { get; protected set; } = 0;
        public int GoogleRequestDeniedCount { get; protected set; } = 0;
        public int GoogleUnknownErrorCount { get; protected set; } = 0;
        public int GoogleErrorMessageCount { get; protected set; } = 0;
        public int DurationCount { get; protected set; } = 0;
        public int NullDurationCount { get; protected set; } = 0;
        public int RanToCompletionCount { get; protected set; } = 0;
        public int FailedToRunToCompletionCount { get; protected set; } = 0;

        public Summary(int number)
        {
            Number = number;
        }

        public virtual void Update(Summary summary)
        {
            TotalRequests += summary.TotalRequests;
            OneWayCount += summary.OneWayCount;
            TwoWayCount += summary.TwoWayCount;
            GoogleOkCount += summary.GoogleOkCount;
            GoogleNotOkCount += summary.GoogleNotOkCount;
            GoogleZeroResultsCount += summary.GoogleZeroResultsCount;
            GoogleMaxWaypointsExceededCount += summary.GoogleMaxWaypointsExceededCount;
            GoogleInvalidRequestCount += summary.GoogleInvalidRequestCount;
            GoogleOverQueryLimitCount += summary.GoogleOverQueryLimitCount;
            GoogleRequestDeniedCount += summary.GoogleRequestDeniedCount;
            GoogleUnknownErrorCount += summary.GoogleUnknownErrorCount;
            GoogleErrorMessageCount += summary.GoogleErrorMessageCount;
            DurationCount += summary.DurationCount;
            NullDurationCount += summary.NullDurationCount;
        }

        public override string ToString()
        {
            return $"===== { Name.ToUpper() } #{ Number } SUMMARY ====={ Environment.NewLine }" +
                $"Total requests: { TotalRequests }{ Environment.NewLine }" +
                $"OK: { GoogleOkCount }{ Environment.NewLine }" +
                $"Has duration: { DurationCount }{ Environment.NewLine }" +
                $"Ran to completion: { RanToCompletionCount }{ Environment.NewLine }" +
                $"One way count: { OneWayCount }{ Environment.NewLine }" +
                $"Two way count: { TwoWayCount }{ Environment.NewLine }" +
                $"Not OK: { GoogleNotOkCount }{ Environment.NewLine }" +
                $"Has duration: { DurationCount }{ Environment.NewLine }" +
                $"No duration: { NullDurationCount }{ Environment.NewLine }" +
                $"Not found: { GoogleNotFoundCount }{ Environment.NewLine }" +
                $"Zero results: { GoogleZeroResultsCount }{ Environment.NewLine }" +
                $"Max waypoints exceeded: { GoogleMaxWaypointsExceededCount }{ Environment.NewLine }" +
                $"Invalid request: { GoogleInvalidRequestCount }{ Environment.NewLine }" +
                $"Over query limit: { GoogleOverQueryLimitCount }{ Environment.NewLine }" +
                $"Request denied: { GoogleRequestDeniedCount }{ Environment.NewLine }" +
                $"Unknown error: { GoogleUnknownErrorCount }{ Environment.NewLine }" +
                $"Error messages: { GoogleErrorMessageCount }{ Environment.NewLine }" +
                $"Failed to complete: { FailedToRunToCompletionCount }{ Environment.NewLine + Environment.NewLine + Environment.NewLine }" +
                $"=============================";
        }
    }

    public class BatchSummary : Summary
    {
        public override string Name { get; protected set; }  = "Batch";

        public BatchSummary(int number, List<Task<Tuple<uint, Edge, EdgeUpdate.UpdateDirections, UpdateInfo, UpdateTime>>> results) : base(number)
        {
            if (results == null)
            {
                return;
            }

            Number = number;

            var resultsAsList = results.Select(t => t.Result).ToList();

            TotalRequests = resultsAsList.Count;
            OneWayCount = resultsAsList.Count(t => t.Item2.IsOneWay);
            TwoWayCount = resultsAsList.Count(t => !t.Item2.IsOneWay);
            GoogleOkCount = resultsAsList.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.OK);
            GoogleNotOkCount = resultsAsList.Count(t => t.Item4.GoogleStatus != DirectionsStatusCodes.OK);
            GoogleNotFoundCount = resultsAsList.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.NOT_FOUND);
            GoogleZeroResultsCount = resultsAsList.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.ZERO_RESULTS);
            GoogleMaxWaypointsExceededCount = resultsAsList.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.MAX_WAYPOINTS_EXCEEDED);
            GoogleInvalidRequestCount = resultsAsList.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.INVALID_REQUEST);
            GoogleOverQueryLimitCount = resultsAsList.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.OVER_QUERY_LIMIT);
            GoogleRequestDeniedCount = resultsAsList.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.REQUEST_DENIED);
            GoogleUnknownErrorCount = resultsAsList.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.UNKNOWN_ERROR);
            GoogleErrorMessageCount = resultsAsList.Count(t => !string.IsNullOrEmpty(t.Item4.GoogleErrorMessage));
            DurationCount = resultsAsList.Count(t => t.Item4.GoogleDuration != null);
            NullDurationCount = resultsAsList.Count(t => t.Item4.GoogleDuration == null);
            RanToCompletionCount = results.Count(t => t.Status == TaskStatus.RanToCompletion);
            FailedToRunToCompletionCount = results.Count(t => t.Status != TaskStatus.RanToCompletion);
        }
    }


    public class ProjectSummary : Summary
    {
        public override string Name { get; protected set; } = "Project";

        public ProjectSummary(int number) : base(number)
        {

        }
    }

    public class ExecutionSummary : Summary
    {
        public override string Name { get; protected set; } = "Execution";

        public ExecutionSummary(int number) : base(number)
        {

        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, base.ToString().Split(Environment.NewLine.ToCharArray()).Skip(1)).Insert(0, $"===== { Name.ToUpper() } SUMMARY =====");
        }
    }
}

/*
        public DirectionsStatusCodes t1 = DirectionsStatusCodes.NOT_FOUND;
        public DirectionsStatusCodes t2 = DirectionsStatusCodes.ZERO_RESULTS;
        public DirectionsStatusCodes t3 = DirectionsStatusCodes.MAX_WAYPOINTS_EXCEEDED;
        public DirectionsStatusCodes t4 = DirectionsStatusCodes.INVALID_REQUEST;
        public DirectionsStatusCodes t5 = DirectionsStatusCodes.OVER_QUERY_LIMIT;
        public DirectionsStatusCodes t6 = DirectionsStatusCodes.REQUEST_DENIED;
        public DirectionsStatusCodes t7 = DirectionsStatusCodes.UNKNOWN_ERROR;
*/
