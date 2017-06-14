using System;

namespace GoogleDataCollection.Logging
{
    public class LogMessage
    {
        public static string DateTimePattern = "HH:mm:ss[fff]";

        public string Message { get; protected set; }
        public string Header { get; protected set; }
        public string Category { get; protected set; }
        public DateTime Timestamp { get; protected set; }
        public Log.PriorityLevels Priority { get; set; }

        public LogMessage(string message, Log.PriorityLevels priority, string category = null, string header = null)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            Timestamp = DateTime.Now;
            Message = message;
            Header = header;
            Category = category;
            Priority = priority;
        }

        public override string ToString()
        {
            return Message;
        }
    }
}
