using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace WindowsEventCollector
{
    public static class EventLogMapper
    {
        public static EventLogData ToEventLogData(this EventLogEntry eventLogEntry)
        {
            return new EventLogData(eventLogEntry.TimeGenerated, eventLogEntry.EntryType, eventLogEntry.Source, eventLogEntry.InstanceId, eventLogEntry.Message);
        }
    }
}
