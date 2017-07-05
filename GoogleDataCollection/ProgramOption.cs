using GoogleDataCollection.DataAccess;
using System.Collections.Generic;

namespace GoogleDataCollection
{
    public class ProgramOption
    {
        public enum OptionTypes : byte { ParseCsv = 1, RunDataCollector, GenerateCsvReport }

        public static List<ProgramOption> DefaultOptions = new List<ProgramOption>
        {
            new ProgramOption(OptionTypes.ParseCsv, "Parse CSV", $"Parse CSV file '{ CsvAccess.DefaultCsvFilename }' and generate new JSON file '{ JsonAccess.DefaultFilename }'."),
            new ProgramOption(OptionTypes.RunDataCollector, "Run data collector", $"Run data collector and save to '{ JsonAccess.DefaultFilename }'."),
            new ProgramOption(OptionTypes.GenerateCsvReport, "Generate CSV report", $"Generate CSV report '{ CsvAccess.DefaultReportFilename }'.")
        };

        public OptionTypes Type { get; protected set; }
        public string Name { get; protected set; }
        public string Description { get; protected set; }

        public ProgramOption(OptionTypes type, string name, string description)
        {
            Type = type;
            Name = name;
            Description = description;
        }

        public override string ToString()
        {
            return $"\tOption { (byte)Type }: { Description }";
        }
    }
}
