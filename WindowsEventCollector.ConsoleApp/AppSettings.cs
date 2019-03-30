using System;
using System.Collections.Generic;
using System.Text;

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
        public List<EventLog> EventLogs { get; set; }
    }

    public class EventLog
    {
        public string Name { get; set; }
        public string Search { get; set; }
        public int Hours { get; set; }
    }
}