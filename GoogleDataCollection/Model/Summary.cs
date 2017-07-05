using GoogleMapsApi.Entities.Directions.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Summary
    {
        public abstract string Name { get; protected set; }

        [JsonProperty(PropertyName = "number", Required = Required.Always)]
        public int Number { get; protected set; }

        [JsonProperty(PropertyName = "totalRequests", Required = Required.Always)]
        public int TotalRequests { get; protected set; } = 0;

        [JsonProperty(PropertyName = "okCount", Required = Required.Always)]
        public int GoogleOkCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "notOkCount", Required = Required.Always)]
        public int GoogleNotOkCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "notFoundCount", Required = Required.Always)]
        public int GoogleNotFoundCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "zeroResultsCount", Required = Required.Always)]
        public int GoogleZeroResultsCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "maxWaypointsExceededCount", Required = Required.Always)]
        public int GoogleMaxWaypointsExceededCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "invalidRequestCount", Required = Required.Always)]
        public int GoogleInvalidRequestCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "overQueryLimitCount", Required = Required.Always)]
        public int GoogleOverQueryLimitCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "requestDeniedCount", Required = Required.Always)]
        public int GoogleRequestDeniedCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "unknownErrorCount", Required = Required.Always)]
        public int GoogleUnknownErrorCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "errorMessageCount", Required = Required.Always)]
        public int GoogleErrorMessageCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "durationCount", Required = Required.Always)]
        public int DurationCount { get; protected set; } = 0;

        [JsonProperty(PropertyName = "nullDurationCount", Required = Required.Always)]
        public int NullDurationCount { get; protected set; } = 0;

        public Summary(int number)
        {
            Number = number;
        }

        public virtual void Update(Summary summary)
        {
            TotalRequests += summary.TotalRequests;
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
                $"Not OK: { GoogleNotOkCount }{ Environment.NewLine }" +
                $"No duration: { NullDurationCount }{ Environment.NewLine }" +
                $"Not found: { GoogleNotFoundCount }{ Environment.NewLine }" +
                $"Zero results: { GoogleZeroResultsCount }{ Environment.NewLine }" +
                $"Max waypoints exceeded: { GoogleMaxWaypointsExceededCount }{ Environment.NewLine }" +
                $"Invalid request: { GoogleInvalidRequestCount }{ Environment.NewLine }" +
                $"Over query limit: { GoogleOverQueryLimitCount }{ Environment.NewLine }" +
                $"Request denied: { GoogleRequestDeniedCount }{ Environment.NewLine }" +
                $"Unknown error: { GoogleUnknownErrorCount }{ Environment.NewLine }" +
                $"Error messages: { GoogleErrorMessageCount }{ Environment.NewLine }" +
                $"============================={ Environment.NewLine }";
        }
    }

    //[JsonObject(MemberSerialization.OptIn)]
    public class BatchSummary : Summary
    {
        public override string Name { get; protected set; }  = "Batch";

        public BatchSummary(int number, List<Tuple<int, Edge, UpdateTime, EdgeUpdate>> results) : base(number)
        {
            if (results == null)
            {
                return;
            }

            Number = number;

            TotalRequests = results.Count;
            GoogleOkCount = results.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.OK);
            GoogleNotOkCount = results.Count(t => t.Item4.GoogleStatus != DirectionsStatusCodes.OK);
            GoogleNotFoundCount = results.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.NOT_FOUND);
            GoogleZeroResultsCount = results.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.ZERO_RESULTS);
            GoogleMaxWaypointsExceededCount = results.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.MAX_WAYPOINTS_EXCEEDED);
            GoogleInvalidRequestCount = results.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.INVALID_REQUEST);
            GoogleOverQueryLimitCount = results.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.OVER_QUERY_LIMIT);
            GoogleRequestDeniedCount = results.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.REQUEST_DENIED);
            GoogleUnknownErrorCount = results.Count(t => t.Item4.GoogleStatus == DirectionsStatusCodes.UNKNOWN_ERROR);
            GoogleErrorMessageCount = results.Count(t => !string.IsNullOrEmpty(t.Item4.GoogleErrorMessage));
            DurationCount = results.Count(t => t.Item4.GoogleDuration != null);
            NullDurationCount = results.Count(t => t.Item4.GoogleDuration == null);
        }
    }

    // Currently not saving the nested project and batch summaries as it makes the JSON quite verbose.
    //[JsonObject(MemberSerialization.OptIn)]
    public class ProjectSummary : Summary
    {
        public override string Name { get; protected set; } = "Project";

        //[JsonProperty(PropertyName = "batchSummaries", Required = Required.Always)]
        public List<BatchSummary> BatchSummaries { get; set; }

        public ProjectSummary(int number) : base(number)
        {
            BatchSummaries = new List<BatchSummary>();
        }

        public override void Update(Summary summary)
        {
            if (!(summary is BatchSummary)) { throw new ArgumentException(); }

            base.Update(summary);

            BatchSummaries.Add((BatchSummary)summary);
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ExecutionSummary : Summary
    {
        public override string Name { get; protected set; } = "Execution";

        //[JsonProperty(PropertyName = "projectSummaries", Required = Required.Always)]
        public List<ProjectSummary> ProjectSummaries { get; set; }

        public ExecutionSummary() : this(1) { }

        public ExecutionSummary(int number) : base(number)
        {
            ProjectSummaries = new List<ProjectSummary>();
        }

        public override void Update(Summary summary)
        {
            if (!(summary is ProjectSummary)) { throw new ArgumentException(); }

            base.Update(summary);

            ProjectSummaries.Add((ProjectSummary)summary);
        }

        public override string ToString()
        {
            return string.Join(Environment.NewLine, base.ToString().Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Skip(1)).Insert(0, $"===== { Name.ToUpper() } SUMMARY ====={ Environment.NewLine }") + $"{ Environment.NewLine }{ Environment.NewLine }";
        }
    }
}
