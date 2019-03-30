using System;

namespace WindowsEventCollector
{
    public class SearchCriteria
    {
        public SearchCriteria(EventLogName logName, string machineName = null, string logSearch = null, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            LogName = logName;
            MachineName = machineName;
            LogSearch = logSearch;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;
        }

        public EventLogName LogName { get; set; }
        public string MachineName { get; set; }
        public string LogSearch { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }
}