using System;

namespace GoogleDataCollection.Logging
{
    public class LogMessage
    {
        //public static string DateTimePattern = "HH:mm:ss[fff]";
        public static string DateTimePattern = "HH:mm:ss:fff";

        public string Message { get; protected set; }
        public string Header { get; protected set; }
        public LogCategory LogCategory{ get; protected set; }
        public DateTime Timestamp { get; protected set; }
        public Log.PriorityLevels Priority { get; set; }

        public LogMessage(string message, Log.PriorityLevels priority, LogCategory logCategory = null, string header = null)
        {
            Timestamp = DateTime.Now;
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Header = header;
            LogCategory = logCategory;
            Priority = priority;
        }

        public override string ToString()
        {
            return $"{ (Header != null ? Header + Environment.NewLine : string.Empty) }" +
                          $"{ (LogCategory != null ? "Category: " + LogCategory + Environment.NewLine : string.Empty) }" +
                          $"[{ Timestamp.ToString(DateTimePattern) }] { Message }{ Environment.NewLine }";
        }
    }
}
