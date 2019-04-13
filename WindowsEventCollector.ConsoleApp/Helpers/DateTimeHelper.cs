using System;
using System.Globalization;

namespace WindowsEventCollector.ConsoleApp.Helpers
{
    public class DateTimeHelper
    {
        public static bool TryParseDateTime(string strDateTime, out DateTime dateTime)
        {
            return DateTime.TryParseExact(strDateTime,
                                          "yyyy-MM-dd HH:mm",
                                          new CultureInfo("en-US"),
                                          DateTimeStyles.None,
                                          out dateTime);
        }
    }
}