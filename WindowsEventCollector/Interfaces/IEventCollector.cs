using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace WindowsEventCollector.Interfaces
{
    public interface IEventCollector
    {
        List<EventLogEntry> GetEventLogEntries(EventLogName eventLog, DateTime? startDateTime, DateTime? endDateTime);
    }
}
