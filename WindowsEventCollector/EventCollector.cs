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
        public List<EventLogEntry> GetEventLogEntries(SearchCriteria searchCriteria)
        {
            EventLog eventLog = new EventLog(searchCriteria.LogName.ToString());
            var eventLogEntryCollection = eventLog.Entries;

            List<EventLogEntry> eventLogEntryList = new List<EventLogEntry>();

            try
            {
                for (int i = 0; i < eventLogEntryCollection.Count; i++)
                {
                    EventLogEntry entry = eventLogEntryCollection[i];

                    if ((!searchCriteria.EndDateTime.HasValue || entry.TimeGenerated <= searchCriteria.EndDateTime) &&
                        (!searchCriteria.StartDateTime.HasValue || entry.TimeGenerated >= searchCriteria.StartDateTime))
                    {
                        if ((searchCriteria.ApplySearch && entry.Message.Contains(searchCriteria.LogContains)) ||
                            searchCriteria.ApplySearch == false)
                        {
                            eventLogEntryList.Add(entry);
                        }
                    }
                }
            }
            catch (SecurityException ex)
            {
                throw new SecurityException($"You do not have permission to access event log: { searchCriteria.LogName }.", ex);
            }

            return eventLogEntryList;
        }
    }
}
