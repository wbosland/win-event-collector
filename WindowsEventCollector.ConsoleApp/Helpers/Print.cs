using System;

namespace WindowsEventCollector.ConsoleApp.Helpers
{
    public class Print
    {
        public static void EnterEndDate()
        {
            WriteLineHelper.WriteToConsole("End: ");
        }

        public static void EnterEventLog()
        {
            WriteLineHelper.WriteToConsole("Event Log: ");
        }

        public static void EnterLogSearch()
        {
            WriteLineHelper.WriteToConsole("Search: ");
        }

        public static void EnterMachineName()
        {
            WriteLineHelper.WriteToConsole("Machine: ");
        }

        public static void EnterStartDate()
        {
            WriteLineHelper.WriteToConsole("Start: ");
        }

        public static void Error(Exception ex)
        {
            WriteLineHelper.WriteLineToConsole($"ERROR: { ex.Message }");
        }

        public static void EventLogEntries(string logName, int count)
        {
            WriteLineHelper.WriteLineToConsole($"{ logName } event log entries: { count }");
        }

        public static void EventLogIsUnknown(string eventLogName)
        {
            WriteLineHelper.WriteLineToConsole($"Event log: '{ eventLogName }' is unknown.");
        }

        public static void ExportedTo(string filePath)
        {
            WriteLineHelper.WriteLineToConsole($"Exported to: { filePath }");
        }

        public static void GettingEventLogData(string logName)
        {
            WriteLineHelper.WriteLineToConsole($"Getting { logName } event log data...");
        }

        public static void HowTo()
        {
            Console.WriteLine("############################# How to use ###################################################");
            Console.WriteLine("### Event Log: Application        #   Input: Application / Security / System / <empty>   ###");
            Console.WriteLine("### Machine:   MachineName        #   Input: <any string> / <empty>                      ###");
            Console.WriteLine("### Search:    Test               #   Input: <any string> / <empty>                      ###");
            Console.WriteLine("### Start:     2019-03-24 00:00   #   Input: <yyyy-mm-dd hh:mm> / <empty>                ###");
            Console.WriteLine("### End:       2019-03-24 13:00   #   Input: <yyyy-mm-dd hh:mm> / <empty>                ###");
            Console.WriteLine("############################################################################################");
        }

        public static void InvalidEndDate()
        {
            WriteLineHelper.WriteLineToConsole("Invalid end date. Please try again.");
        }

        public static void InvalidStartDate()
        {
            WriteLineHelper.WriteLineToConsole("Invalid start date. Please try again.");
        }

        public static void PressKeyToExit()
        {
            WriteLineHelper.WriteLineToConsole("Press a key to exit.");
        }

        public static void SettingsToConsole(EventLogSettings defaultSettings)
        {
            EnterEventLog();
            string eventLogName = defaultSettings.LogName;
            Console.WriteLine(eventLogName);

            EnterMachineName();
            string machineName = defaultSettings.Machine;
            Console.WriteLine(machineName);

            EnterLogSearch();
            string logSearch = defaultSettings.Search;
            Console.WriteLine(logSearch);

            EnterStartDate();
            DateTime startDateTime = DateTime.Now.Subtract(TimeSpan.FromHours(defaultSettings.Hours));
            Console.WriteLine(startDateTime);

            EnterEndDate();
            Console.WriteLine("");
        }

        public static void StartCollecting()
        {
            WriteLineHelper.WriteLineToConsole("Start collecting Windows events.");
        }
    }
}