using Newtonsoft.Json;
using System.Collections.Generic;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class ErrorContainer
    {
        [JsonProperty(PropertyName = "errors", Required = Required.Always)]
        List<PointToPointError> Errors { get; set; }

        public ErrorContainer()
        {
            Errors = new List<PointToPointError>();
        }
    }
}
