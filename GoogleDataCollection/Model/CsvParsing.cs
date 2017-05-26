using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GoogleDataCollection.Model
{
    [JsonObject(MemberSerialization.OptIn)]
    public class CsvParsing
    {
        [JsonProperty(PropertyName = "errors", Required = Required.Always)]
        public List<CsvParsingError> Errors { get; set; }

        [JsonProperty(PropertyName = "parseTime", Required = Required.Always)]
        public DateTime Time { get; set; }

        public CsvParsing()
        {
            Errors = new List<CsvParsingError>();
        }
    }
}
