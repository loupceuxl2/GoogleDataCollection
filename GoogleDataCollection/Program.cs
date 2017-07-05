using GoogleDataCollection.DataAccess;
using GoogleDataCollection.Helper;
using GoogleDataCollection.Logging;
using GoogleDataCollection.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static GoogleDataCollection.ProgramOption;

namespace GoogleDataCollection
{
    internal class Program
    {
        // !!!IMPORTANT: If you want to be able to select multiple options, then you may want to first add/hard code your APIs to Project.DefaultProject() static method. Don't forget to recompile!
        // !IMPORTANT [v2.0]: I've coded the program to be run as many times as you like without corrupting the data, i.e., unsuccessful requests are ignored (do not get saved). See summaries output to the console for success ratios.
        // !IMPORTANT [v2.0]: Albeit highly unlikely if you manage to obtain enough Google API keys from other sources to perform a complete cycle, instead of exiting the program will simply start a new cycle. Respective projects will only quit once an OVER_QUERY_LIMIT or REQUEST_DENIED is found, or if Project.MaxRequests is reached.
        // !IMPORTANT [v2.0]: Data collection is split by project. Each project has a maximum number of requests.
        //      To optimise data collection projects are first filled with the edges which have had the lowest count of updates (data retrieval requests), REGARDLESS of which specific update.
        //      So, basically adding a new UpdateTime mid cycle will obviously skew the cycle. If necessary to add a new UpdateTime, it is recommended to add at the end of the list.
        // !IMPORTANT [v2.0]: Individual projects will now quit upon having found their first (respective) OVER_QUERY_LIMIT or REQUEST_DENIED.
        // !IMPORTANT [v2.0]: Requests are now also asynchronous (supported by Google). Previously only projects were. This should make things 3-5x faster.
        // NOTE: For the sake of simplicty two way streets (edges) are parsed as two separate JSON edges.

        // DONE: Add note regarding adding and/or removing UpdateTimes to the 'qld_network.json'.
        // DONE: Import logging from Caliburn Micro app (AutoProgramCM).
        private static void Main(string[] args)
        {
            var initialConsoleMode = ConsoleQuickEdit.GetConsoleMode();
            ConsoleQuickEdit.Disable();     // Mouse clicks on the console stops code execution, therefore to fix: Disable quick edit.

            try
            {
                Log.GlobalLog = new Log(new FileInfo($@"{ AppDomain.CurrentDomain.BaseDirectory }\{ Log.DefaultGlobalLogFilename }"))
                {
                    //Output = OutputFormats.File | OutputFormats.Console | OutputFormats.Debugger,
                    Output = Log.OutputFormats.Console,
                    FileWriteMode = Log.FileWriteModes.Overwrite,
                    ConsolePriority = Log.PriorityLevels.Medium,            // Highly recommended, any lower will significantly decrease performance. For lower priorities consider using file logging instead which runs as a background task.
                    FilePriority = Log.PriorityLevels.UltraLow,             // File logging runs as a background task and should not affect operations, although if the application is closed 'too early' some log messages may be lost. Also, it has to be enabled (Output = OutputFormats.File).
                    DebuggerPriority = Log.PriorityLevels.UltraLow
                };

                // Uncomment to disable global logging.
/*                
                Logging.Log.GlobalLog.Disable();
*/

                // Uncomment to disable individual project logging (currently file based).
/*                
                Project.EnableLogging = false;
*/

                RunOptions(RetrieveUserOptions());
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception");
                Console.WriteLine($"{e}");
            }

            // Set quick edit back to whatever it was.
            if (initialConsoleMode != null)
            { 
                ConsoleQuickEdit.SetConsoleMode((uint)initialConsoleMode);
            }

            Console.WriteLine($"Press any key to exit...");
            Console.ReadLine();
        }

        private static List<OptionTypes> RetrieveUserOptions()
        {
            List<OptionTypes> validOptions;

            while (true)
            {
                Console.WriteLine($"Please select from the following options.{ Environment.NewLine }{ Environment.NewLine }If selecting more than one option, separate by whitespace, e.g., '2 3' to select options 2 and 3.{ Environment.NewLine }" +
                    $"{ string.Join($"{ Environment.NewLine }", DefaultOptions) }{ Environment.NewLine }");

                var selectedOptions = Console.ReadLine().Split(new char[0], StringSplitOptions.RemoveEmptyEntries);

                // REFERENCE: https://stackoverflow.com/questions/4961675/select-parsed-int-if-string-was-parseable-to-int
                validOptions = selectedOptions
                    .Select(s =>
                    {
                        bool success = byte.TryParse(s, out byte value);
                        return new { value, success };
                    })
                    .Where(pair => pair.success && Enum.IsDefined(typeof(OptionTypes), pair.value))
                    .Select(pair => pair.value)
                    .Cast<OptionTypes>()
                    .Distinct()
                    .ToList();

                if (validOptions.Count > 0 && validOptions.Count == selectedOptions.Length) { break; }
            }

            return validOptions;
        }

        private static void RunOptions(List<OptionTypes> options)
        {
            options.OrderBy(o => o).ToList().ForEach(o => RunOption(o));
        }

        // TO DO[OPTIONAL]: Clean up this method.
        private static void RunOption(OptionTypes option)
        {
            switch (option)
            {
                case OptionTypes.ParseCsv:
                    var container = CsvAccess.ParseCsv();

                    if (container == null) { return; }

                    // Add default update times (hard coded).
                    container.UpdateTimes.AddRange(UpdateTime.DefaultUpdateTimes);
                    // Add default projects (hard coded). Currently empty!!!
                    container.Projects.AddRange(Project.DefaultProjects());

                    File.WriteAllText(JsonAccess.DefaultFilename, JsonConvert.SerializeObject(container, Formatting.Indented));

                    return;

                case OptionTypes.RunDataCollector:
                    var data = JsonAccess.DeserializeEdges();
                    var status = -100;

                    if (data == null) { return; }

                    Task.Run(() => status = GoogleAccess.RunDataCollector(data).Result).Wait();

                    if (status == 0) { JsonAccess.SerializeEdges(data); }

                    return;

                case OptionTypes.GenerateCsvReport:
                    CsvAccess.GenerateCsvReportGroupedByUpdateTime();

                    return;

                default:
                    return;
            }
        }
    }
}

