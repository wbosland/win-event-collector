using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

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