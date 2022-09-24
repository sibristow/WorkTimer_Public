using System;
using WorkTimer4.API.Data;

namespace WorkTimer4.API.Connectors
{
    public enum RollingInterval
    {
        /// <summary>
        /// File will never be rolled
        /// </summary>
        Never = 0,

        /// <summary>
        /// File will be rolled each year
        /// </summary>
        Year = 1,

        /// <summary>
        /// File will be rolled each month
        /// </summary>
        Month = 2,

        /// <summary>
        /// File will be rolled each week
        /// </summary>
        Week = 3,

        /// <summary>
        /// File will be rolled each day
        /// </summary>
        Day = 4
    }

    public static class RollingIntervalExtensions
    {
        public static string GetDateFormat(this RollingInterval rollingInterval)
        {
            switch (rollingInterval)
            {
                case RollingInterval.Never:
                    return string.Empty;

                case RollingInterval.Year:
                    return "yyyy";

                case RollingInterval.Month:
                    return "yyyyMM";

                case RollingInterval.Week:
                case RollingInterval.Day:
                    return "yyyyMMdd";

                default:
                    throw new ArgumentException("Invalid rolling interval");
            }
        }

        public static DateTimeOffset? GetCurrentTimestamp(this RollingInterval rollingInterval, DateTimeOffset now)
        {
            switch (rollingInterval)
            {
                case RollingInterval.Never:
                    return null;

                case RollingInterval.Year:
                    // align to 1st Jan
                    return new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset);

                case RollingInterval.Month:
                    // align to 1st of month
                    return new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset);

                case RollingInterval.Week:
                    // align to previous Monday
                    return new DateTimeOffset(now.DateTime.GetPreviousMonday());

                case RollingInterval.Day:
                    return now;

                default:
                    throw new ArgumentException("Invalid rolling interval");
            }
        }

        public static DateTimeOffset? GetNextTimestamp(this RollingInterval rollingInterval, DateTimeOffset now)
        {
            switch (rollingInterval)
            {
                case RollingInterval.Never:
                    return null;

                case RollingInterval.Year:
                    // align to 1st Jan
                    return new DateTimeOffset(now.Year, 1, 1, 0, 0, 0, now.Offset).AddYears(1);

                case RollingInterval.Month:
                    // align to 1st of month
                    return new DateTimeOffset(now.Year, now.Month, 1, 0, 0, 0, now.Offset).AddMonths(1);

                case RollingInterval.Week:
                    // align to next Monday
                    return new DateTimeOffset(now.DateTime.GetNextDayOfWeek(DayOfWeek.Monday));

                case RollingInterval.Day:
                    // align to tomorrow
                    return now.AddDays(1);

                default:
                    throw new ArgumentException("Invalid rolling interval");
            }
        }
    }
}
