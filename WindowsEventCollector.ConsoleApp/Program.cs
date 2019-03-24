using Microsoft.Extensions.Configuration;
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
        static List<SearchCriteria> searchCriterias;
        static SearchCriteria lastSearchCriteria;
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

            EnterSearchCriteria();

            WriteLineToConsole("Start collecting Windows events.");

            Collect();

            WriteLineToConsole("Press a key to exit.");

            Console.ReadKey();
        }

        private static void EnterSearchCriteria()
        {
            searchCriterias = new List<SearchCriteria>();
            bool done = false;

            while (!done)
            {
                WriteToConsole("Event Log: ");
                string eventLogName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(eventLogName) &&
                    searchCriterias.Count > 0)
                {
                    done = true;
                    break;
                }

                if (Enum.TryParse(eventLogName, out EventLogName eventLogNameEnum))
                {
                    lastSearchCriteria = new SearchCriteria(eventLogNameEnum, "");
                    searchCriterias.Add(lastSearchCriteria);
                    EnterLogContains();
                }
                else
                {
                    WriteLineToConsole("This event log name is not supported.");
                }
            }
        }

        private static void EnterLogContains()
        {
            WriteToConsole("Search: ");
            lastSearchCriteria.LogContains = Console.ReadLine();

            if (!string.IsNullOrWhiteSpace(lastSearchCriteria.LogContains))
            {
                lastSearchCriteria.ApplySearch = true;
            }

            EnterStartDate();
            EnterEndDate();
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
                    lastSearchCriteria.StartDateTime = null;
                    break;
                }

                if (TryParseDateTime(strStartDateTime, out DateTime dateTime))
                {
                    startDateTimeIsValid = true;
                    lastSearchCriteria.StartDateTime = dateTime;
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
                    lastSearchCriteria.EndDateTime = null;
                    break;
                }

                if (TryParseDateTime(strEndDateTime, out DateTime dateTime))
                {
                    endDateTimeIsValid = true;
                    lastSearchCriteria.EndDateTime = dateTime;
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

            foreach (var searchCriteria in searchCriterias)
            {
                tasks.Add(Task.Run(() =>
                {
                    WriteLineToConsole($"Getting { searchCriteria.LogName.ToString() } event log data...");
                    List<EventLogData> eventLogs = eventCollector.GetEventLogEntries(searchCriteria).Select(entry => entry.ToEventLogData()).ToList();
                    WriteLineToConsole($"{ searchCriteria.LogName.ToString() } event log entries: { eventLogs.Count }");
                    string filePath = $@"{ Program.filePath }\{ searchCriteria.LogName }EventLogs.xlsx";
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
