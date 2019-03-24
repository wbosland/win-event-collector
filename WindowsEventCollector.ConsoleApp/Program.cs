using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using WindowsEventCollector.Interfaces;

namespace WindowsEventCollector.ConsoleApp
{
    class Program
    {
        static string filePath;
        static List<EventLogName> eventLogNames;
        static DateTime? startDateTime;
        static DateTime? endDateTime;
        static IEventCollector eventCollector;
        static IExportService exportService;

        static void Main(string[] args)
        {
            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", true, true)
                .Build();
            filePath = config["filePath"];

            eventCollector = new EventCollector();
            exportService = new ExportService();

            Console.WriteLine("############################# How to use ###################################################");
            Console.WriteLine("### Event Log: Application        #   Input: Application / Security / System / <empty>   ###");
            Console.WriteLine("### Start:     2019-03-24 00:00   #   Input: <yyyy-mm-dd hh:mm> / <empty>                ###");
            Console.WriteLine("### End:       2019-03-24 13:00   #   Input: <yyyy-mm-dd hh:mm> / <empty>                ###");
            Console.WriteLine("############################################################################################");
            //Console.WriteLine("Enter a start and end date including time.");
            //Console.WriteLine("E.g. start: '2019-02-17 16:00' and end: '2019-02-18 16:00' (without the quotes).");

            EnterEventLogName();
            EnterStartDate();
            EnterEndDate();

            WriteLineToConsole("Start collecting Windows events.");
            Collect();

            WriteLineToConsole("Press a key to exit.");

            Console.ReadKey();
        }

        private static void EnterEventLogName()
        {
            eventLogNames = new List<EventLogName>();
            bool done = false;

            while (!done)
            {
                WriteToConsole("Event Log: ");
                string eventLogName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(eventLogName) &&
                    eventLogNames.Count > 0)
                {
                    done = true;
                    break;
                }

                if (Enum.TryParse(eventLogName, out EventLogName eventLogNameEnum))
                {
                    eventLogNames.Add(eventLogNameEnum);
                }
                else
                {
                    WriteLineToConsole("This event log name is not supported.");
                }
            }
        }

        private static void EnterStartDate()
        {
            bool startDateTimeIsValid = false;

            while (!startDateTimeIsValid)
            {
                WriteToConsole("Start: ");
                string strStartDateTime = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(strStartDateTime))
                {
                    startDateTimeIsValid = true;
                    startDateTime = null;
                    break;
                }

                if (TryParseDateTime(strStartDateTime, out DateTime dateTime))
                {
                    startDateTimeIsValid = true;
                    startDateTime = dateTime;
                }
                else
                {
                    WriteLineToConsole("Invalid start date. Please try again.");
                }
            }
        }

        private static void EnterEndDate()
        {
            bool endDateTimeIsValid = false;

            while (!endDateTimeIsValid)
            {
                WriteToConsole("End: ");
                string strEndDateTime = Console.ReadLine();

                if (String.IsNullOrWhiteSpace(strEndDateTime))
                {
                    endDateTimeIsValid = true;
                    endDateTime = null;
                    break;
                }

                if (TryParseDateTime(strEndDateTime, out DateTime dateTime))
                {
                    endDateTimeIsValid = true;
                    endDateTime = dateTime;
                }
                else
                {
                    WriteLineToConsole("Invalid end date. Please try again.");
                }
            }
        }

        private static void Collect()
        {
            var tasks = new List<Task>();

            foreach (var eventLogName in eventLogNames)
            {
                tasks.Add(Task.Run(() =>
                {
                    WriteLineToConsole($"Getting { eventLogName.ToString() } event log data...");
                    List<EventLogData> eventLogs = eventCollector.GetEventLogEntries(eventLogName, startDateTime, endDateTime).Select(entry => entry.ToEventLogData()).ToList();
                    WriteLineToConsole($"{ eventLogName.ToString() } event log entries: { eventLogs.Count }");
                    string filePath = $@"{ Program.filePath }\{ eventLogName }EventLogs.xlsx";
                    exportService.ExportToExcel(eventLogs, filePath);
                    WriteLineToConsole($"Exported to: { filePath }");
                }));
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ae)
            {
                ae.Handle((ex) =>
                {
                    if (ex is SystemException)
                    {
                        WriteLineToConsole($"ERROR: { ex.Message }");
                        return true;
                    }
                    return false;
                });
            }
        }

        private static bool TryParseDateTime(string strDateTime, out DateTime dateTime)
        {
            return DateTime.TryParseExact(strDateTime, 
                                          "yyyy-MM-dd HH:mm", 
                                          new CultureInfo("en-US"), 
                                          DateTimeStyles.None, 
                                          out dateTime);
        }

        private static void WriteLineToConsole(object message)
        {
            Console.WriteLine($"[{ Thread.CurrentThread.ManagedThreadId }] { message }");
        }

        private static void WriteToConsole(object message)
        {
            Console.Write($"[{ Thread.CurrentThread.ManagedThreadId }] { message }");
        }
    }
}
