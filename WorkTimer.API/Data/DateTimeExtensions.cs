using System;

namespace WorkTimer4.API.Data
{
    public static class DateTimeExtensions
    {
        /// <summary>
        /// Gets the date of the previous Monday
        /// </summary>
        /// <param name="fromDate">date to get previous Monday from</param>
        /// <returns></returns>
        public static DateTime GetPreviousMonday(this DateTime fromDate)
        {
            var daysBack = (int)fromDate.DayOfWeek;
            if (daysBack == (int)DayOfWeek.Sunday)
                daysBack = 7;

            daysBack -= (int)DayOfWeek.Monday;
            daysBack *= -1;

            return fromDate.AddDays(daysBack);
        }

        public static DateTime GetNextMonday(this DateTime fromDate)
        {
            return GetNextDayOfWeek(fromDate, DayOfWeek.Monday);
        }

        //public static DateTime GetPreviousDayOfWeek(this DateTime fromDate, DayOfWeek dayOfWeek)
        //{
        //    var daysBack = (int)fromDate.DayOfWeek;
        //    if (daysBack == (int)DayOfWeek.Sunday)
        //        daysBack = 7;

        //    daysBack -= (int)DayOfWeek.Monday;
        //    daysBack *= -1;

        //    return fromDate.AddDays(daysBack);
        //}

        public static DateTime GetNextDayOfWeek(this DateTime fromDate, DayOfWeek dayOfWeek)
        {
            var fromDoW = (int)fromDate.DayOfWeek;
            if (fromDoW == (int)DayOfWeek.Sunday)
                fromDoW = 7;

            var daysFwd = 7 - fromDoW + (dayOfWeek == DayOfWeek.Sunday ? 7 : (int)dayOfWeek);

            return fromDate.AddDays(daysFwd);
        }

        /// <summary>
        /// Returns a new <see cref="DateTimeOffset"/> at the start of the day
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        public static DateTimeOffset ToStartOfDay(this DateTimeOffset dateTimeOffset)
        {
            return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 0, 0, 0, dateTimeOffset.Offset);
        }

        /// <summary>
        /// Returns a new <see cref="DateTimeOffset"/> at the end of the day
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        public static DateTimeOffset ToEndOfDay(this DateTimeOffset dateTimeOffset)
        {
            return new DateTimeOffset(dateTimeOffset.Year, dateTimeOffset.Month, dateTimeOffset.Day, 23, 59, 59, 999, dateTimeOffset.Offset);
        }

        /// <summary>
        /// Returns the datetime formatted for use as the aggregation dictionary key
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        internal static string AsAggregationKey(this DateTimeOffset dateTimeOffset)
        {
            return string.Format("d{0}", dateTimeOffset.ToString("yyyyMMdd"));
        }
    }
}
