using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Mvc;
using Devesprit.Core.Localization;
using Devesprit.Data.Enums;

namespace Devesprit.Utilities.Extensions
{
    public static partial class OtherExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> seenKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static DateTime? AddTimePeriodToDateTime(this DateTime date, TimePeriodType? periodType, int? period)
        {
            if (periodType == null || period == null)
            {
                return null;
            }
            switch (periodType)
            {
                case TimePeriodType.Hour:
                    return date.AddHours(period.Value);
                case TimePeriodType.Day:
                    return date.AddDays(period.Value);
                case TimePeriodType.Month:
                    return date.AddMonths(period.Value);
                case TimePeriodType.Year:
                    return date.AddYears(period.Value);
                default:
                    throw new ArgumentOutOfRangeException(nameof(periodType), periodType, null);
            }
        }

        public static string TimePeriodToString(this TimePeriodType periodType, int period)
        {
            var localization = DependencyResolver.Current.GetService<ILocalizationService>();
            if (period <= 0)
            {
                return localization.GetResource("Never");
            }
            switch (periodType)
            {
                case TimePeriodType.Hour:
                    return period + " " + localization.GetResource("Hours");
                case TimePeriodType.Day:
                    return period + " " + localization.GetResource("Days");
                case TimePeriodType.Month:
                    return period + " " + localization.GetResource("Months");
                case TimePeriodType.Year:
                    return period + " " + localization.GetResource("Years");
                default:
                    throw new ArgumentOutOfRangeException(nameof(periodType), periodType, null);
            }
        }

        public static bool HasFlagFast(this Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            // Not as good as the .NET 4 version of this function, but should be good enough
            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(
                    $"Enumeration type mismatch.  The flag is of type '{value.GetType()}', was expecting '{variable.GetType()}'.");
            }

            ulong num = Convert.ToUInt64(value);
            return ((Convert.ToUInt64(variable) & num) == num);
        }
    }
}
