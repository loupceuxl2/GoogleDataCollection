using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace GoogleDataCollection.Logging
{
    public class Log
    {
        public enum PriorityLevels
        {
            UltraLow,
            Low,
            Medium,
            High,
            UltraHigh
        }

        public enum FileModes
        {
            Overwrite,
            Append
        }

        [Flags]
        public enum OutputFormats : byte
        {
            File = 1,
            Debugger = 2
        }

        // REFERENCE: http://stackoverflow.com/questions/8447/what-does-the-flags-enum-attribute-mean-in-c#
        [Flags]
        public enum LogOptions
        {
            LogTimeStamp = 1
        }

        public static Log GlobalLog = new Log(new FileInfo(@"D:\Project1\Programming\WPF\AutoProgramCM\AutoProgramCM.Core\Data\Logs\global_log.txt"),
                PriorityLevels.UltraLow, FileModes.Overwrite, PriorityLevels.UltraLow, OutputFormats.Debugger | OutputFormats.File);

        public static PriorityLevels DefaultFilePriority { get; set; } = PriorityLevels.UltraLow;
        public static PriorityLevels DefaultDebuggerPriority { get; set; } = PriorityLevels.UltraLow;
        public static FileModes DefaultFileMode { get; set; } = FileModes.Append;

        public PriorityLevels FilePriority { get; set; }
        public PriorityLevels DebuggerPriority { get; set; }
        public FileModes FileMode { get; set; }
        public OutputFormats? Output { get; set; }
        public bool IsEnabled { get; protected set; }
        public FileInfo FileInfo { get; set; }
        public uint WriteToFileCount { get; protected set; }
        protected List<LogMessage> Messages { get; set; }

        public event LogMessageAddedEventHandler LogMessageAdded;

        // File & debugger.
        public Log(FileInfo textFileInfo, PriorityLevels filePriority, FileModes fileMode, PriorityLevels debugPriority, OutputFormats? output, bool enable = true)
        {
            FileInfo = textFileInfo;
            FilePriority = filePriority;
            FileMode = fileMode;
            DebuggerPriority = debugPriority;
            Output = output;
            IsEnabled = enable;
            WriteToFileCount = 0;
            Messages = new List<LogMessage>();
        }

        // File only.
        public Log(FileInfo textFileInfo, PriorityLevels filePriority, FileModes fileMode, bool enable = true) : this(textFileInfo, filePriority, fileMode, DefaultDebuggerPriority, OutputFormats.File, enable) { }

        // Debugger only.
        public Log(PriorityLevels debuggerPriority, bool enable = true) : this(null, DefaultFilePriority, DefaultFileMode, debuggerPriority, OutputFormats.Debugger, enable) { }

        // No outputs.
        public Log(bool enable = true) : this(null, DefaultFilePriority, DefaultFileMode, DefaultDebuggerPriority, null, enable) { }

        public void AddToLog(LogMessage message, bool writeToOutputs = true)
        {
            if (!IsEnabled) { return ; }

            Messages.Add(message);

            OnLogMessageAdded(new LogMessageAddedEventArgs { Message = message });

            if (writeToOutputs) { WriteToOutputs(message);}
        }

        public void ClearLogMessages()
        {
            Messages.Clear();
        }

        public void Enable()
        {
            IsEnabled = true;
        }

        public void Disable()
        {
            IsEnabled = false;
        }

        public void FlushMessages()
        {
            if (!IsEnabled) { return; }

            foreach (var message in Messages)
            {
                WriteToOutputs(message);
            }
        }

        public List<LogMessage> FilterMessagesByPriority(PriorityLevels matchingPriority)
        {
            return Messages.FindAll(m => HasPriority(matchingPriority, m.Priority));
        }

        public List<LogMessage> FilterMessagesByAtLeastPriority(PriorityLevels matchingPriority)
        {
            return Messages.FindAll(m => HasAtLeastPriority(matchingPriority, m.Priority));
        }

        protected void WriteToOutputs(LogMessage message)
        {
            if (Output == null) {  return; }

            if (((OutputFormats)Output).HasFlag(OutputFormats.File))
            {
                WriteToFile(message);
            }

            if (((OutputFormats) Output).HasFlag(OutputFormats.Debugger))
            {
                WriteToDebugger(message);
            }
        }

        protected void WriteToFile(LogMessage message)
        {
            if (!HasAtLeastPriority(FilePriority, message.Priority) || FileInfo == null)
            {
                return;
            }

            if (FileMode == FileModes.Overwrite && WriteToFileCount == 0)
            {
                File.WriteAllText(FileInfo.FullName, message.Message);
            }
            else
            {
                File.AppendAllText(FileInfo.FullName, message.Message);
            }

            WriteToFileCount++;
        }

        protected void WriteToDebugger(LogMessage message)
        {
            if (!HasAtLeastPriority(DebuggerPriority, message.Priority))
            {
                return;
            }

            Debug.Write(message.Message);
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
