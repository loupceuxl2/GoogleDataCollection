using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoogleDataCollection.Logging
{
    public class Log
    {
        public static readonly string DefaultGlobalLogFilename = "global_log.txt";
        public static readonly FileInfo DefaultFileInfo = new FileInfo($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ DefaultGlobalLogFilename }");

        public enum PriorityLevels
        {
            UltraLow,
            Low,
            Medium,
            High,
            UltraHigh
        }

        public enum WriteModes
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

        [Flags]
        public enum LogOptions
        {
            LogTimeStamp = 1
        }

        public PriorityLevels FilePriority { get; set; }
        public PriorityLevels DebuggerPriority { get; set; }
        public PriorityLevels ConsolePriority { get; set; }
        public WriteModes WriteMode { get; set; }
        public OutputFormats? Output { get; set; }
        public FileInfo FileInfo { get; set; }
        public bool IsEnabled { get; protected set; }
        public uint WriteToFileCount { get; protected set; }
        public ConcurrentBag<LogMessage> Messages { get; protected set; }

        public event LogMessageAddedEventHandler LogMessageAdded;

        public Log(FileInfo fileInfo, PriorityLevels filePriority = PriorityLevels.UltraLow, WriteModes writeMode = WriteModes.Overwrite, PriorityLevels debugPriority = PriorityLevels.UltraLow, PriorityLevels consolePriority = PriorityLevels.UltraLow, OutputFormats? output = OutputFormats.File | OutputFormats.Debugger, bool enable = true)
        {
            FileInfo = fileInfo;
            FilePriority = filePriority;
            WriteMode = writeMode;
            DebuggerPriority = debugPriority;
            ConsolePriority = consolePriority;
            Output = output;
            IsEnabled = enable;
            WriteToFileCount = 0;
            Messages = new ConcurrentBag<LogMessage>();
        }

        public Log(PriorityLevels filePriority = PriorityLevels.UltraLow, WriteModes writeMode = WriteModes.Overwrite, PriorityLevels debugPriority = PriorityLevels.UltraLow, PriorityLevels consolePriority = PriorityLevels.UltraLow, OutputFormats? output = OutputFormats.File | OutputFormats.Debugger, bool enable = true)
                : this(new FileInfo($"{ AppDomain.CurrentDomain.BaseDirectory }\\{ DefaultGlobalLogFilename }"), filePriority, writeMode, debugPriority, consolePriority, output, enable) { }

        public void AddToLog(LogMessage message, bool writeToOutputs = true)
        {
            if (!IsEnabled) { return ; }

            Messages.Add(message);

            OnLogMessageAdded(new LogMessageAddedEventArgs { Message = message });

            if (writeToOutputs) { WriteToOutputs(message);}
        }
/*
        public void ClearLogMessages()
        {
            Messages.Clear();
        }
*/
        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

/*
        public void FlushMessages()
        {
            foreach (var message in Messages)
            {
                WriteToOutputs(message);
            }

            ClearLogMessages();
        }
*/
        public List<LogMessage> GetMessagesByPriority(PriorityLevels matchingPriority)
        {
            return Messages.ToList().FindAll(m => HasPriority(matchingPriority, m.Priority));
        }

        public List<LogMessage> GetMessagesByAtLeastPriority(PriorityLevels matchingPriority)
        {
            return Messages.ToList().FindAll(m => HasAtLeastPriority(matchingPriority, m.Priority));
        }

        private static void WriteTextAsync(string filePath, string text)
        {
            var encodedText = Encoding.Unicode.GetBytes(text);

            using (var sourceStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, false))
            {
                sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }

        protected void WriteToOutputs(LogMessage message)
        {
            if (Output == null) {  return; }
            
            if (((OutputFormats)Output).HasFlag(OutputFormats.File))
            {
                WriteTextAsync($"{ AppDomain.CurrentDomain.BaseDirectory }\\fdsfds.txt", message.ToString());
            }
/*
            if (((OutputFormats)Output).HasFlag(OutputFormats.File))
            {
                WriteToFile(message);
            }
*/
            if (((OutputFormats) Output).HasFlag(OutputFormats.Debugger))
            {
                WriteToDebugger(message);
            }

            if (((OutputFormats)Output).HasFlag(OutputFormats.Console))
            {
                WriteToConsole(message);
            }
        }

        // Change to flush to file.
        protected void WriteToFile(LogMessage logMessage)
        {
            if (!HasAtLeastPriority(FilePriority, logMessage.Priority) || FileInfo == null)
            {
                return;
            }

            var encodedText = Encoding.Unicode.GetBytes(logMessage.ToString());

            if (WriteMode == WriteModes.Overwrite && WriteToFileCount == 0)
            {
                using (var sourceStream = new FileStream(FileInfo.FullName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, false))
                {
                    sourceStream.Write(encodedText, 0, encodedText.Length);
                    //sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                }

                File.WriteAllText(FileInfo.FullName, $"{ logMessage }");
            }
            else
            {
                using (var sourceStream = new FileStream(FileInfo.FullName, FileMode.Append, FileAccess.Write, FileShare.ReadWrite, 4096, false))
                {
                    sourceStream.Write(encodedText, 0, encodedText.Length);
                    //sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
                }

                File.AppendAllText(FileInfo.FullName, $"{ logMessage }");
            }

            WriteToFileCount++;
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
            var handler = LogMessageAdded;

            handler?.Invoke(this, e);
        }

        public static bool HasPriority(PriorityLevels matchingPriority, PriorityLevels messagePriority)
        {
            return messagePriority == matchingPriority;
        }

        public static bool HasAtLeastPriority(PriorityLevels matchingPriority, PriorityLevels messagePriority)
        {
            return messagePriority >= matchingPriority;
        }
    }

    public class LogMessageAddedEventArgs : EventArgs
    {
        public LogMessage Message { get; set; }
    }

    public delegate void LogMessageAddedEventHandler(object sender, LogMessageAddedEventArgs e);
}
