using Newtonsoft.Json;
using System;

namespace GoogleDataCollection.Model
{
    // TO DO: Add (all, i.e., batch, project, overall) summaries here?
    [JsonObject(MemberSerialization.OptIn)]
    public class UpdateSession
    {
        [JsonProperty(PropertyName = "runTimeStartedAt", Required = Required.Always)]
        public DateTime RunTimeStartedAt { get; set; }

        [JsonProperty(PropertyName = "runTimeCompletedAt", Required = Required.Always)]
        public DateTime RunTimeCompletedAt { get; set; }

        [JsonProperty(PropertyName = "executionSummary", Required = Required.Always)]
        public ExecutionSummary ExecutionSummary { get; set; }
    }
}
