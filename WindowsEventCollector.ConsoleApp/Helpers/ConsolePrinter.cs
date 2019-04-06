using System;
using System.Threading;

namespace WindowsEventCollector.ConsoleApp.Helpers
{
    public class ConsolePrinter
    {
        public static void PrintEnterEndDate()
        {
            WriteToConsole("End: ");
        }

        public static void PrintEnterEventLog()
        {
            WriteToConsole("Event Log: ");
        }

        public static void PrintEnterLogSearch()
        {
            WriteToConsole("Search: ");
        }

        public static void PrintEnterMachineName()
        {
            WriteToConsole("Machine: ");
        }

        public static void PrintEnterStartDate()
        {
            WriteToConsole("Start: ");
        }

        public static void PrintError(Exception ex)
        {
            WriteLineToConsole($"ERROR: { ex.Message }");
        }

        public static void PrintEventLogEntries(string logName, int count)
        {
            WriteLineToConsole($"{ logName } event log entries: { count }");
        }

        public static void PrintEventLogIsUnknown(string eventLogName)
        {
            WriteLineToConsole($"Event log: '{ eventLogName }' is unknown.");
        }

        public static void PrintExportedTo(string filePath)
        {
            WriteLineToConsole($"Exported to: { filePath }");
        }

        public static void PrintGettingEventLogData(string logName)
        {
            WriteLineToConsole($"Getting { logName } event log data...");
        }

        public static void PrintHowTo()
        {
            Console.WriteLine("############################# How to use ###################################################");
            Console.WriteLine("### Event Log: Application        #   Input: Application / Security / System / <empty>   ###");
            Console.WriteLine("### Machine:   MachineName        #   Input: <any string> / <empty>                      ###");
            Console.WriteLine("### Search:    Test               #   Input: <any string> / <empty>                      ###");
            Console.WriteLine("### Start:     2019-03-24 00:00   #   Input: <yyyy-mm-dd hh:mm> / <empty>                ###");
            Console.WriteLine("### End:       2019-03-24 13:00   #   Input: <yyyy-mm-dd hh:mm> / <empty>                ###");
            Console.WriteLine("############################################################################################");
        }

        public static void PrintInvalidEndDate()
        {
            WriteLineToConsole("Invalid end date. Please try again.");
        }

        public static void PrintInvalidStartDate()
        {
            WriteLineToConsole("Invalid start date. Please try again.");
        }

        public static void PrintPressKeyToExit()
        {
            WriteLineToConsole("Press a key to exit.");
        }

        public static void PrintSettingsToConsole(EventLogSettings defaultSettings)
        {
            PrintEnterEventLog();
            string eventLogName = defaultSettings.LogName;
            Console.WriteLine(eventLogName);

            PrintEnterMachineName();
            string machineName = defaultSettings.Machine;
            Console.WriteLine(machineName);

            PrintEnterLogSearch();
            string logSearch = defaultSettings.Search;
            Console.WriteLine(logSearch);

            PrintEnterStartDate();
            DateTime startDateTime = DateTime.Now.Subtract(TimeSpan.FromHours(defaultSettings.Hours));
            Console.WriteLine(startDateTime);

            PrintEnterEndDate();
            Console.WriteLine("");
        }

        public static void PrintStartCollecting()
        {
            WriteLineToConsole("Start collecting Windows events.");
        }

        public static void WriteLineToConsole(object message)
        {
            Console.WriteLine($"[{ Thread.CurrentThread.ManagedThreadId }] { message }");
        }

        public static void WriteToConsole(object message)
        {
            Console.Write($"[{ Thread.CurrentThread.ManagedThreadId }] { message }");
        }
    }
}