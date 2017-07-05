using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GoogleDataCollection.Logging
{
    public class Log
    {
        public static readonly string DefaultGlobalLogFilename = "global_log.txt";
        public static readonly FileInfo DefaultFileInfo = new FileInfo($@"{ AppDomain.CurrentDomain.BaseDirectory }\{ DefaultGlobalLogFilename }");
        public static Log GlobalLog = new Log(new FileInfo($@"{ AppDomain.CurrentDomain.BaseDirectory }\global_log.txt"))
        {
            //Output = OutputFormats.File | OutputFormats.Console | OutputFormats.Debugger,
            Output = OutputFormats.Console,
            FileWriteMode = FileWriteModes.Overwrite,
            ConsolePriority = PriorityLevels.Medium,
            FilePriority = PriorityLevels.UltraLow,
            DebuggerPriority = PriorityLevels.UltraLow
        };

        private static uint _logCount = 0;
        private static readonly BlockingCollection<Tuple<LogMessage, Log>> GlobalMessages = new BlockingCollection<Tuple<LogMessage, Log>>();
        private static readonly CancellationTokenSource LogToFileCancellationToken = new CancellationTokenSource();

        public enum PriorityLevels
        {
            UltraLow,
            Low,
            Medium,
            High,
            UltraHigh
        }

        public enum FileWriteModes
        {
            Overwrite,
            Append
        }

        // REFERENCE: http://stackoverflow.com/questions/8447/what-does-the-flags-enum-attribute-mean-in-c#
        [Flags]
        public enum OutputFormats : byte
        {
            File = 1,
            Debugger = 2,
            Console = 4
        }

        public event LogMessageAddedEventHandler LogMessageAdded;

        public PriorityLevels FilePriority { get; set; }
        public PriorityLevels DebuggerPriority { get; set; }
        public PriorityLevels ConsolePriority { get; set; }
        public FileWriteModes FileWriteMode { get; set; }
        public OutputFormats Output { get; set; }
        public FileInfo FileInfo { get; protected set; }
        public bool IsEnabled { get; protected set; }
        public uint WriteToFileCount { get; protected set; }
        public ConcurrentBag<LogMessage> Messages { get; protected set; }

        public Log(FileInfo fileInfo, PriorityLevels filePriority = PriorityLevels.UltraLow, FileWriteModes writeMode = FileWriteModes.Overwrite, PriorityLevels debugPriority = PriorityLevels.UltraLow, PriorityLevels consolePriority = PriorityLevels.UltraLow, OutputFormats output = OutputFormats.File | OutputFormats.Debugger, bool enable = true)
        {
            FileInfo = fileInfo;
            FilePriority = filePriority;
            FileWriteMode = writeMode;
            DebuggerPriority = debugPriority;
            ConsolePriority = consolePriority;
            Output = output;
            IsEnabled = enable;
            WriteToFileCount = 0;
            Messages = new ConcurrentBag<LogMessage>();

            _logCount++;
            
            if (_logCount == 1) 
            { 
                LogToFileBackgroundTask();
            }
        }

        public Log(PriorityLevels filePriority = PriorityLevels.UltraLow, FileWriteModes writeMode = FileWriteModes.Overwrite, PriorityLevels debugPriority = PriorityLevels.UltraLow, PriorityLevels consolePriority = PriorityLevels.UltraLow, OutputFormats output = OutputFormats.File | OutputFormats.Debugger, bool enable = true)
                : this(new FileInfo($@"{ AppDomain.CurrentDomain.BaseDirectory }\{ DefaultGlobalLogFilename }"), filePriority, writeMode, debugPriority, consolePriority, output, enable) { }

        public void AddToLog(LogMessage message, bool writeToOutputs = true)
        {
            if (!IsEnabled) { return ; }

            Messages.Add(message);

            OnLogMessageAdded(new LogMessageAddedEventArgs { Message = message });

            if (writeToOutputs) { WriteToOutputs(message);}
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public List<LogMessage> GetMessagesByPriority(PriorityLevels matchingPriority)
        {
            return Messages.ToList().FindAll(m => HasPriority(matchingPriority, m.Priority));
        }

        public List<LogMessage> GetMessagesByAtLeastPriority(PriorityLevels matchingPriority)
        {
            return Messages.ToList().FindAll(m => HasAtLeastPriority(matchingPriority, m.Priority));
        }

        protected void WriteToOutputs(LogMessage message)
        {
            if (Output.HasFlag(OutputFormats.File))
            {
                GlobalMessages.Add(new Tuple<LogMessage, Log>(message, this));
            }

            if (Output.HasFlag(OutputFormats.Debugger))
            {
                WriteToDebugger(message);
            }

            if (Output.HasFlag(OutputFormats.Console))
            {
                WriteToConsole(message);
            }
        }

        protected void WriteToDebugger(LogMessage logMessage)
        {
            if (!HasAtLeastPriority(DebuggerPriority, logMessage.Priority))
            {
                return;
            }

            Debug.Write($"{ logMessage }");
        }

        protected void WriteToConsole(LogMessage message)
        {
            if (!HasAtLeastPriority(ConsolePriority, message.Priority))
            {
                return;
            }

            Console.Write($"{ message }");
        }

        protected virtual void OnLogMessageAdded(LogMessageAddedEventArgs e)
        {
            LogMessageAdded?.Invoke(this, e);
        }

        public static bool HasPriority(PriorityLevels matchingPriority, PriorityLevels messagePriority)
        {
            return messagePriority == matchingPriority;
        }

        public static bool HasAtLeastPriority(PriorityLevels matchingPriority, PriorityLevels messagePriority)
        {
            return messagePriority >= matchingPriority;
        }


        private static void LogToFileBackgroundTask()
        {
            Task.Run(() =>
            {
                while (true)
                {
                    foreach (var tuple in GlobalMessages.GetConsumingEnumerable())
                    {
                        if (LogToFileCancellationToken.IsCancellationRequested) { return; }

                        if (tuple.Item2.IsEnabled 
                            && tuple.Item2.Output.HasFlag(OutputFormats.File)
                            && HasAtLeastPriority(tuple.Item2.FilePriority, tuple.Item1.Priority)
                            && tuple.Item2.FileInfo != null)
                        {
                            WriteToFile(tuple.Item1, tuple.Item2);
                        }
                    }
                }
            });
        }

        private static void WriteToFile(LogMessage logMessage, Log log)
        {
            if (log.FileWriteMode == FileWriteModes.Overwrite && log.WriteToFileCount == 0)
            {
                File.WriteAllText(log.FileInfo.FullName, $"{ logMessage }");
            }
            else
            {
                File.AppendAllText(log.FileInfo.FullName, $"{ logMessage }");
            }

            log.WriteToFileCount++;
        }
    }

    public class LogMessageAddedEventArgs : EventArgs
    {
        public LogMessage Message { get; set; }
    }

    public delegate void LogMessageAddedEventHandler(object sender, LogMessageAddedEventArgs e);
}
