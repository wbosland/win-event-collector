using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WindowsEventCollector.ConsoleApp.Helpers;
using WindowsEventCollector.Interfaces;

namespace WindowsEventCollector.ConsoleApp
{
    class Program
    {
        private static AppSettings appSettings;
        private static List<SearchCriteria> searchCriterias = new List<SearchCriteria>();
        private static SearchCriteria lastSearchCriteria;
        private static IEventCollector eventCollector = new EventCollector();
        private static IExportService exportService = new ExportService();

        static void Main(string[] args)
        {
            ReadAppSettings();

            Print.HowTo();

            if (appSettings.UseStartupSettings)
            {
                foreach (var eventLogSettings in appSettings.StartupSettings.EventLogSettings)
                {
                    CreateSearchCriteria(eventLogSettings);
                }
            }
            else
            {
                EnterSearchCriteria();
            }

            Print.StartCollecting();

            Collect();

            Print.PressKeyToExit();

            Console.ReadKey();
        }

        private static void CreateSearchCriteria(EventLogSettings eventLogSettings)
        {
            Print.SettingsToConsole(eventLogSettings);

            if (Enum.TryParse(eventLogSettings.LogName, out EventLogName eventLogNameEnum))
            {
                lastSearchCriteria = new SearchCriteria(logName: eventLogNameEnum, 
                                                        machineName: eventLogSettings.Machine, 
                                                        logSearch: eventLogSettings.Search,
                                                        startDateTime: DateTime.Now.Subtract(TimeSpan.FromHours(eventLogSettings.Hours)));
                searchCriterias.Add(lastSearchCriteria);
            }
            else
            {
                Print.EventLogIsUnknown(eventLogSettings.LogName);
            }
        }

        private static void EnterSearchCriteria()
        {
            bool done = false;

            while (!done)
            {
                Print.EnterEventLog();
                string eventLogName = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(eventLogName) &&
                    searchCriterias.Count > 0)
                {
                    done = true;
                    break;
                }

                if (Enum.TryParse(eventLogName, out EventLogName eventLogNameEnum))
                {
                    lastSearchCriteria = new SearchCriteria(logName: eventLogNameEnum);
                    EnterMachineName();
                    EnterLogSearch();
                    EnterStartDate();
                    EnterEndDate();
                    searchCriterias.Add(lastSearchCriteria);
                }
                else
                {
                    Print.EventLogIsUnknown(eventLogName);
                }
            }
        }

        private static void EnterMachineName()
        {
            Print.EnterMachineName();
            lastSearchCriteria.MachineName = Console.ReadLine();
        }

        private static void EnterLogSearch()
        {
            Print.EnterLogSearch();
            lastSearchCriteria.LogSearch = Console.ReadLine();
        }

        private static void EnterStartDate()
        {
            bool startDateTimeIsValid = false;

            while (!startDateTimeIsValid)
            {
                Print.EnterStartDate();
                string strStartDateTime = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(strStartDateTime))
                {
                    startDateTimeIsValid = true;
                    lastSearchCriteria.StartDateTime = null;
                    break;
                }

                if (DateTimeHelper.TryParseDateTime(strStartDateTime, out DateTime dateTime))
                {
                    startDateTimeIsValid = true;
                    lastSearchCriteria.StartDateTime = dateTime;
                }
                else
                {
                    Print.InvalidStartDate();
                }
            }
        }

        private static void EnterEndDate()
        {
            bool endDateTimeIsValid = false;

            while (!endDateTimeIsValid)
            {
                Print.EnterEndDate();
                string strEndDateTime = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(strEndDateTime))
                {
                    endDateTimeIsValid = true;
                    lastSearchCriteria.EndDateTime = null;
                    break;
                }

                if (DateTimeHelper.TryParseDateTime(strEndDateTime, out DateTime dateTime))
                {
                    endDateTimeIsValid = true;
                    lastSearchCriteria.EndDateTime = dateTime;
                }
                else
                {
                    Print.InvalidEndDate();
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
                    Print.GettingEventLogData(searchCriteria.LogName.ToString());

                    List<EventLogData> eventLogs = eventCollector.GetEventLogEntries(searchCriteria).Select(entry => entry.ToEventLogData()).ToList();

                    Print.EventLogEntries(searchCriteria.LogName.ToString(), eventLogs.Count());

                    string filePath = $@"{ appSettings.FilePath }\{ searchCriteria.LogName }EventLogs.xlsx";

                    exportService.ExportToExcel(eventLogs, filePath);

                    Print.ExportedTo(filePath);
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
                        Print.Error(ex);
                        return true;
                    }
                    return false;
                });
            }
        }

        private static void ReadAppSettings()
        {
            IConfiguration config = new ConfigurationBuilder()
                            .AddJsonFile("appsettings.json", true, true)
                            .Build();
            appSettings = new AppSettings();
            config.GetSection("settings").Bind(appSettings);
        }
    }
}