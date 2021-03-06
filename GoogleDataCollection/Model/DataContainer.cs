﻿using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class DataContainer
    {
        [JsonProperty(PropertyName = "csvParsing", Required = Required.Default)]
        public CsvParsing CsvParsing { get; set; }

        [JsonProperty(PropertyName = "updateTimes", Required = Required.Always)]
        public List<UpdateTime> UpdateTimes { get; set; }

        [JsonProperty(PropertyName = "projects", Required = Required.Default)]
        public List<Project> Projects { get; set; }

        [JsonProperty(PropertyName = "updateSessions", Required = Required.Always)]
        public List<UpdateSession> UpdateSessions { get; set; }

        [JsonProperty(PropertyName = "edges", Required = Required.Always)]
        public List<Edge> Edges { get; set; }

        public DataContainer()
        {
            Edges = new List<Edge>();
            UpdateSessions = new List<UpdateSession>();
            UpdateTimes = new List<UpdateTime>();
            Projects = new List<Project>();
        }
    }
}
