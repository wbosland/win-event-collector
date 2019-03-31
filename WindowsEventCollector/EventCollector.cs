using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security;
using WindowsEventCollector.Interfaces;

namespace WindowsEventCollector
{
    public class EventCollector : IEventCollector
    {
        public List<EventLogEntry> GetEventLogEntries(SearchCriteria searchCriteria)
        {
            EventLog eventLog;
            bool applySearch = string.IsNullOrWhiteSpace(searchCriteria.LogSearch) == false;

            if (string.IsNullOrWhiteSpace(searchCriteria.MachineName))
            {
                eventLog = new EventLog(searchCriteria.LogName.ToString());
            }
            else
            {
                eventLog = new EventLog(searchCriteria.LogName.ToString(), searchCriteria.MachineName);
            }

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
                        if ((applySearch && entry.Message.Contains(searchCriteria.LogSearch)) ||
                             applySearch == false)
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
            catch (IOException ex)
            {
                throw new IOException($"Not able to connect to machine: { searchCriteria.MachineName }", ex);
            }

            return eventLogEntryList;
        }
    }
}
