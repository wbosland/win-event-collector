using System;

namespace WindowsEventCollector
{
    public class SearchCriteria
    {
        public SearchCriteria(EventLogName logName, string logContains, DateTime? startDateTime = null, DateTime? endDateTime = null)
        {
            LogName = logName;
            LogContains = logContains;
            StartDateTime = startDateTime;
            EndDateTime = endDateTime;

            if (!string.IsNullOrWhiteSpace(logContains))
            {
                ApplySearch = true;
            }
        }

        public EventLogName LogName { get; set; }
        public string LogContains { get; set; }
        public bool ApplySearch { get; set; }
        public DateTime? StartDateTime { get; set; }
        public DateTime? EndDateTime { get; set; }
    }
}
