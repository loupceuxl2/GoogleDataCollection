using GoogleDataCollection.DataAccess;
using GoogleDataCollection.Helper;
using GoogleDataCollection.Logging;
using GoogleDataCollection.Model;
using Newtonsoft.Json;
using System;
using System.IO;

namespace GoogleDataCollection
{
    internal class Program
    {
        // TO DO: Add note regarding deletion of TimeBracket will corrupt data.
        // DONE: Import logging from Caliburn Micro app (AutoProgramCM).
        private static void Main(string[] args)
        {
            var initialConsoleMode = ConsoleQuickEdit.GetConsoleMode();
            ConsoleQuickEdit.Disable();     // Mouse clicks on the console stops code execution, therefore disable quick edit.

            try
            {
                Log.GlobalLog = new Log(new FileInfo($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ Log.DefaultGlobalLogFilename }"))
                {
                    //Output = OutputFormats.File | OutputFormats.Console | OutputFormats.Debugger,
                    Output = Log.OutputFormats.Console,
                    FileWriteMode = Log.FileWriteModes.Overwrite,
                    ConsolePriority = Log.PriorityLevels.Medium,       // Highly recommended, any lower will significantly decrease performance. For lower priorities consider using file logging instead which runs as a background task.
                    FilePriority = Log.PriorityLevels.UltraLow,             // File logging runs as a background task and should not affect operations, although if the application is closed 'too early' some log messages may be lost. Also, it has to be enabled (Output = OutputFormats.File).
                    DebuggerPriority = Log.PriorityLevels.UltraLow
                };

                // Uncomment to disable global logging.
/*                
                Logging.Log.GlobalLog.Disable();
*/

                // Uncomment to disable project logging.
/*                
                Project.EnableLogging = false;
*/

                var container = CsvAccess.ParseCsv();
                container.UpdateTimes.AddRange(UpdateTime.DefaultUpdateTimes);      // Add default update times (hard coded).
                container.Projects.AddRange(Project.GenerateTestProjects());        // If testing, add some (real) projects.
                File.WriteAllText(JsonAccess.DefaultFilename, JsonConvert.SerializeObject(container, Formatting.Indented));

                var data = JsonAccess.DeserializeEdges();
/*
                GoogleAccess.RunDataCollector(data).Wait();
                JsonAccess.SerializeEdges(data);
*/
                CsvAccess.GenerateCsvReportGroupedByUpdateTime(data);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception");
                Console.WriteLine($"{e}");
            }

            if (initialConsoleMode != null)
            { 
                ConsoleQuickEdit.SetConsoleMode((uint)initialConsoleMode);
            }

            // TO DO: Uncomment for release version.
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
        }
    }
}

