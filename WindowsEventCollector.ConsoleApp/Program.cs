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
        static AppSettings appSettings;
        static List<SearchCriteria> searchCriterias = new List<SearchCriteria>();
        static SearchCriteria lastSearchCriteria;
        static IEventCollector eventCollector = new EventCollector();
        static IExportService exportService = new ExportService();

        static void Main(string[] args)
        {
            ReadAppSettings();

            Console.WriteLine("############################# How to use ###################################################");
            Console.WriteLine("### Event Log: Application        #   Input: Application / Security / System / <empty>   ###");
            Console.WriteLine("### Machine:   MachineName        #   Input: <any string> / <empty>                      ###");
            Console.WriteLine("### Search:    Test               #   Input: <any string> / <empty>                      ###");
            Console.WriteLine("### Start:     2019-03-24 00:00   #   Input: <yyyy-mm-dd hh:mm> / <empty>                ###");
            Console.WriteLine("### End:       2019-03-24 13:00   #   Input: <yyyy-mm-dd hh:mm> / <empty>                ###");
            Console.WriteLine("############################################################################################");

            if (appSettings.UseStartupSettings)
            {
                foreach (var eventLogSettings in appSettings.StartupSettings.EventLogSettings)
                {
                    EnterSearchCriteria(eventLogSettings);
                }
            }
            else
            {
                EnterSearchCriteria();
            }

            WriteLineToConsole("Start collecting Windows events.");

            Collect();

            WriteLineToConsole("Press a key to exit.");

            Console.ReadKey();
        }

        private static void ReadAppSettings()
        {
            IConfiguration config = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json", true, true)
                            .Build();
            appSettings = new AppSettings();
            config.GetSection("settings").Bind(appSettings);
        }

        private static void EnterSearchCriteria(EventLogSettings defaultSettings = null)
        {
            bool loop = true;

            while (loop)
            {
                string eventLogName;
                string machineName;

                WriteToConsole("Event Log: ");

                if (defaultSettings != null)
                {
                    loop = false;
                    eventLogName = defaultSettings.LogName;

                    Console.WriteLine(eventLogName);
                }
                else
                {
                    eventLogName = Console.ReadLine();
                }

                if (string.IsNullOrWhiteSpace(eventLogName) &&
                    searchCriterias.Count > 0)
                {
                    loop = false;
                    break;
                }

                WriteToConsole("Machine: ");

                if (defaultSettings != null)
                {
                    machineName = defaultSettings.Machine;
                    Console.WriteLine(machineName);
                }
                else
                {
                    machineName = Console.ReadLine();
                }

                if (Enum.TryParse(eventLogName, out EventLogName eventLogNameEnum))
                {
                    lastSearchCriteria = new SearchCriteria(eventLogNameEnum, machineName);
                    searchCriterias.Add(lastSearchCriteria);
                    EnterLogSearch(defaultSettings);
                }
                else
                {
                    WriteLineToConsole("This event log name is not supported.");
                }
            }
        }

        private static void EnterLogSearch(EventLogSettings defaultSettings = null)
        {
            WriteToConsole("Search: ");

            if (defaultSettings != null)
            {
                lastSearchCriteria.LogSearch = defaultSettings.Search;
                Console.WriteLine(lastSearchCriteria.LogSearch);
            }
            else
            {
                lastSearchCriteria.LogSearch = Console.ReadLine();
            }

            EnterStartDate(defaultSettings);
            EnterEndDate(defaultSettings);
        }

        private static void EnterStartDate(EventLogSettings defaultSettings = null)
        {
            bool startDateTimeIsValid = false;

            while (!startDateTimeIsValid)
            {
                WriteToConsole("Start: ");
                string strStartDateTime;

                if (defaultSettings != null)
                {
                    startDateTimeIsValid = true;
                    lastSearchCriteria.StartDateTime = DateTime.Now.Subtract(TimeSpan.FromHours(defaultSettings.Hours));
                    Console.WriteLine(lastSearchCriteria.StartDateTime);
                    break;
                }
                else
                {
                    strStartDateTime = Console.ReadLine();
                }

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

        private static void EnterEndDate(EventLogSettings defaultSettings = null)
        {
            bool endDateTimeIsValid = false;

            while (!endDateTimeIsValid)
            {
                WriteToConsole("End: ");
                string strEndDateTime;

                if (defaultSettings != null)
                {
                    endDateTimeIsValid = true;
                    lastSearchCriteria.EndDateTime = null;
                    Console.WriteLine("");
                    break;
                }
                else
                {
                    strEndDateTime = Console.ReadLine();
                }

                if (string.IsNullOrWhiteSpace(strEndDateTime))
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
                    string filePath = $@"{ appSettings.FilePath }\{ searchCriteria.LogName }EventLogs.xlsx";
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
