using System.Collections.Generic;

namespace WindowsEventCollector.ConsoleApp
{
    public class AppSettings
    {
        public string FilePath { get; set; }
        public bool OverwriteFile { get; set; }
        public bool UseStartupSettings { get; set; }
        public StartupSettings StartupSettings { get; set; }
        
    }

    public class StartupSettings
    {
        public List<EventLogSettings> EventLogSettings { get; set; }
    }

    public class EventLogSettings
    {
        public string LogName { get; set; }
        public string Machine { get; set; }
        public string Search { get; set; }
        public int Hours { get; set; }
    }
}