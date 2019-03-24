using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.Collections;
using System.Security;
using System.Threading;
using WindowsEventCollector.Interfaces;

namespace WindowsEventCollector
{
    public class EventCollector : IEventCollector
    {
        public List<EventLogEntry> GetEventLogEntries(EventLogName eventLogName, DateTime? startDateTime, DateTime? endDateTime)
        {
            EventLog eventLog = new EventLog(eventLogName.ToString());
            var eventLogEntryCollection = eventLog.Entries;

            List<EventLogEntry> eventLogEntryList = new List<EventLogEntry>();

            try
            {
                for (int i = 0; i < eventLogEntryCollection.Count; i++)
                {
                    EventLogEntry entry = eventLogEntryCollection[i];

                    if ((!endDateTime.HasValue || entry.TimeGenerated <= endDateTime) &&
                        (!startDateTime.HasValue || entry.TimeGenerated >= startDateTime))
                    {
                        eventLogEntryList.Add(entry);
                    }
                }
            }
            catch (SecurityException ex)
            {
                throw new SecurityException($"You do not have permission to access event log: { eventLogName }.", ex);
            }

            return eventLogEntryList;
        }
    }
}
