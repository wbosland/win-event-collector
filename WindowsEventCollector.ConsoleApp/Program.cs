using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
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

            ConsolePrinter.PrintHowTo();

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

            ConsolePrinter.PrintStartCollecting();

            Collect();

            ConsolePrinter.PrintPressKeyToExit();

            Console.ReadKey();
        }

        private static void CreateSearchCriteria(EventLogSettings eventLogSettings)
        {
            ConsolePrinter.PrintSettingsToConsole(eventLogSettings);

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
                ConsolePrinter.PrintEventLogIsUnknown(eventLogSettings.LogName);
            }
        }

        private static void EnterSearchCriteria()
        {
            bool done = false;

            while (!done)
            {
                ConsolePrinter.PrintEnterEventLog();
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
                    ConsolePrinter.PrintEventLogIsUnknown(eventLogName);
                }
            }
        }

        private static void EnterMachineName()
        {
            ConsolePrinter.PrintEnterMachineName();
            lastSearchCriteria.MachineName = Console.ReadLine();
        }

        private static void EnterLogSearch()
        {
            ConsolePrinter.PrintEnterLogSearch();
            lastSearchCriteria.LogSearch = Console.ReadLine();
        }

        private static void EnterStartDate()
        {
            bool startDateTimeIsValid = false;

            while (!startDateTimeIsValid)
            {
                ConsolePrinter.PrintEnterStartDate();
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
                    ConsolePrinter.PrintInvalidStartDate();
                }
            }
        }

        private static void EnterEndDate()
        {
            bool endDateTimeIsValid = false;

            while (!endDateTimeIsValid)
            {
                ConsolePrinter.PrintEnterEndDate();
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
                    ConsolePrinter.PrintInvalidEndDate();
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
                    ConsolePrinter.PrintGettingEventLogData(searchCriteria.LogName.ToString());

                    List<EventLogData> eventLogs = eventCollector.GetEventLogEntries(searchCriteria).Select(entry => entry.ToEventLogData()).ToList();

                    ConsolePrinter.PrintEventLogEntries(searchCriteria.LogName.ToString(), eventLogs.Count());

                    string filePath = GetFilePath(searchCriteria.LogName.ToString());

                    exportService.ExportToExcel(eventLogs, filePath);

                    ConsolePrinter.PrintExportedTo(filePath);
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
                        ConsolePrinter.PrintError(ex);
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

        private static string GetFilePath(string logName)
        {
            string unixTimeStamp = "";

            if (!appSettings.OverwriteFile)
            {
                unixTimeStamp = "-" + DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            }

            string filePath = $@"{ appSettings.FilePath }\{ logName }EventLogs{ unixTimeStamp }.xlsx";
            return filePath;
        }
    }
}