using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DataContainer
    {
        [JsonProperty(PropertyName = "csvParsing", Required = Required.Default)]
        public CsvParsing CsvParsing { get; set; }

        [JsonProperty(PropertyName = "timeBrackets", Required = Required.Always)]
        public List<TimeBracket> TimeBrackets { get; set; }

        [JsonProperty(PropertyName = "projects", Required = Required.Default)]
        public List<Project> Projects { get; set; }

        [JsonProperty(PropertyName = "updateSessions", Required = Required.Always)]
        public List<UpdateSession> UpdateSessions { get; set; }

        [JsonProperty(PropertyName = "edges", Required = Required.Always)]
        public List<Edge> Edges { get; set; }

        public static Dictionary<uint, Edge> EdgesToDictionary(DataContainer container)
        {
            return container.Edges.ToDictionary(x => x.Fid, x => x);
        }

        public DataContainer()
        {
            Edges = new List<Edge>();
            UpdateSessions = new List<UpdateSession>();
            TimeBrackets = new List<TimeBracket>();
            Projects = new List<Project>();
        }
    }
}
