using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace WindowsEventCollector.Interfaces
{
    public interface IEventCollector
    {
        List<EventLogEntry> GetEventLogEntries(SearchCriteria searchCriteria);
    }
}
