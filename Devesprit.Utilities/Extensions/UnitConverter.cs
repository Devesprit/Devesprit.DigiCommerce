using System;
using System.Web.Mvc;
using Devesprit.Core.Localization;

namespace Devesprit.Utilities.Extensions
{
    public static partial class UnitConverter
    {
        public static string TimeAgo(this DateTime dateTime)
        { 
            var localization = DependencyResolver.Current.GetService<ILocalizationService>();
            var span = DateTime.Now.Subtract(dateTime);
            if (span.Days > 365)
            {
                var years = (span.Days / 365);
                if (span.Days % 365 != 0)
                    years += 1;
                return $"{localization.GetResource("About")} {years} {(years == 1 ? localization.GetResource("Year") : localization.GetResource("Years"))} {localization.GetResource("Ago")}";
            }
            if (span.Days > 30)
            {
                var months = (span.Days / 30);
                if (span.Days % 31 != 0)
                    months += 1;
                return $"{localization.GetResource("About")} {months} {(months == 1 ? localization.GetResource("Month") : localization.GetResource("Months"))} {localization.GetResource("Ago")}";
            }
            if (span.Days > 0)
                return $"{localization.GetResource("About")} {span.Days} {(span.Days == 1 ? localization.GetResource("Day") : localization.GetResource("Days"))} {localization.GetResource("Ago")}";
            if (span.Hours > 0)
                return $"{localization.GetResource("About")} {span.Hours} {(span.Hours == 1 ? localization.GetResource("Hour") : localization.GetResource("Hours"))} {localization.GetResource("Ago")}";
            if (span.Minutes > 0)
                return $"{localization.GetResource("About")} {span.Minutes} {(span.Minutes == 1 ? localization.GetResource("Minute") : localization.GetResource("Minutes"))} {localization.GetResource("Ago")}";
            if (span.Seconds > 5)
                return $"{localization.GetResource("About")} {span.Seconds} {localization.GetResource("Seconds")} {localization.GetResource("Ago")}";
            if (span.Seconds <= 5)
                return localization.GetResource("JustNow");
            return string.Empty;
        }

        public static string FormatNumber(this int n)
        {
            if (n < 1000)
                return n.ToString();

            if (n < 10000)
                return $"{n - 5:#,.##}K";

            if (n < 100000)
                return $"{n - 50:#,.#}K";

            if (n < 1000000)
                return $"{n - 500:#,.}K";

            if (n < 10000000)
                return $"{n - 5000:#,,.##}M";

            if (n < 100000000)
                return $"{n - 50000:#,,.#}M";

            if (n < 1000000000)
                return $"{n - 500000:#,,.}M";

            return $"{n - 5000000:#,,,.##}B";
        }

        public static string FileSizeSuffix(this long value, int decimalPlaces = 1)
        {
            string[] sizeSuffixes =
                { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };
            if (value < 0)
            {
                { return "-" + FileSizeSuffix(-value); }
            }

            if (value == 0)
            {
                { return "0.0 bytes"; }
            }

            // mag is 0 for bytes, 1 for KB, 2, for MB, etc.
            int mag = (int)Math.Log(value, 1024);

            // 1L << (mag * 10) == 2 ^ (10 * mag) 
            // [i.e. the number of bytes in the unit corresponding to mag]
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            // make adjustment when the value is large enough that
            // it would round up to 1000 or more
            if (Math.Round(adjustedSize, decimalPlaces) >= 1000)
            {
                {
                    mag += 1;
                    adjustedSize /= 1024;
                }
            }

            return string.Format("{0:n" + decimalPlaces + "} {1}",
                adjustedSize,
                sizeSuffixes[mag]);
        }
    }
}
