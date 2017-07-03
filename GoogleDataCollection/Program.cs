using GoogleDataCollection.DataAccess;
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
            try
            {
                Log.GlobalLog = new Log(new FileInfo($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ Log.DefaultGlobalLogFilename }"))
                {
                    //Output = OutputFormats.File | OutputFormats.Console | OutputFormats.Debugger,
                    Output = Log.OutputFormats.Console,
                    WriteMode = Log.WriteModes.Overwrite,
                    ConsolePriority = Log.PriorityLevels.Medium,            // Highly recommended, any lower will significantly decrease operations' performance.
                    FilePriority = Log.PriorityLevels.Low,             // File logging runs as a background task and should not affect operations, although if the application is closed all log messages may not have been processed. Also, it has to be enabled (Output = OutputFormats.File).
                    DebuggerPriority = Log.PriorityLevels.UltraLow
                };

                // Uncomment to disable global logging.
/*                
                //Logging.Log.GlobalLog.Disable();
*/


                // !IMPORTANT: Uncomment to get a new (clean) Qld network JSON file. This will overwrite any existing "qld_network.json".
/*  
                var container = CsvAccess.ParseCsv();
                container.UpdateTimes.AddRange(UpdateTime.DefaultUpdateTimes);      // Add default update times (hard coded).
                container.Projects.AddRange(Project.GenerateTestProjects());        // If testing, add some (real) projects.
                File.WriteAllText(JsonAccess.DefaultFilename, JsonConvert.SerializeObject(container, Formatting.Indented));
*/
                var data = JsonAccess.DeserializeEdges();
                GoogleAccess.RunDataCollector(data).Wait();
                File.WriteAllText($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ JsonAccess.DefaultFilename }", JsonConvert.SerializeObject(data, Formatting.Indented));

            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception");
                Console.WriteLine($"{e}");
            }

            // TO DO: Uncomment for release version.
            Console.WriteLine("Press enter to close...");
            Console.ReadLine();
        }
    }
}

