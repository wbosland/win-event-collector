using System;
using System.Diagnostics;

namespace WindowsEventCollector
{
    public class EventLogData
    {
        public EventLogData(DateTime dateCreated, EventLogEntryType level, string source, long eventId, string message)
        {
            DateCreated = dateCreated;
            Level = level.ToString();
            Source = source;
            EventId = eventId;
            Message = message;
        }

        public DateTime DateCreated { get; set; }
        public string Level { get; set; }
        public string Source { get; set; }
        public long EventId { get; set; }
        public string Message { get; set; }
    }
}
