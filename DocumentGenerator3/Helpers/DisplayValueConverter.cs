using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentGenerator3.Helpers
{
    internal static class DisplayValueConverter
    {
        internal static string ChangeDateToDisplayDate(string date, string timezone)
        {
            DateTime dateValue;
            CultureInfo enUS = new CultureInfo("en-US");

            var isGeneralDate = DateTime.TryParseExact(date, "g", enUS, DateTimeStyles.None, out dateValue);

            var isUtcDate = DateTime.TryParseExact(date, "o", enUS, DateTimeStyles.None, out dateValue);

            if (isUtcDate || isGeneralDate)
            {
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
                var dateInNewTimeZone = TimeZoneInfo.ConvertTimeFromUtc(dateValue, timeZoneInfo);

                return dateInNewTimeZone.ToString();
            }

            return null;
        }

        internal static int? ChangeStringToInt(string data)
        {
            int result = 0;
            var isNumber = Int32.TryParse(data, out result);
            if (!isNumber)
            {
                return null;
            }

            return result;
        }
    }
}
