using System;

namespace WorkTimer4.TimesheetView
{
    internal record struct DateHours
    {
        /// <summary>
        /// Gets the total hours
        /// </summary>
        public double TotalHours { get; }

        /// <summary>
        /// Gets the total hours rounded to the nearest quarter of an hour
        /// </summary>
        public double QuarterHours
        {
            get
            {
                return Math.Round(this.TotalHours * 4, MidpointRounding.ToEven) * 0.25;
            }
        }


        public string FormattedHours
        {
            get
            {
                return string.Format("{0:F3} / {1:F2}", this.TotalHours, this.QuarterHours);
            }
        }



        public DateHours(double hours)
        {
            this.TotalHours = hours;
        }


        /// <summary>
        /// Returns a new <see cref="DateHours"/> with the sum of the hours
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DateHours operator +(DateHours a, DateHours b)
        {
            return a.Add(b);
        }

        /// <summary>
        /// Returns a new <see cref="DateHours"/> with the sum of the hours
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static DateHours operator +(DateHours a, double b)
        {
            return a.Add(b);
        }


        /// <summary>
        /// Returns a new <see cref="DateHours"/> with the sum of the hours
        /// </summary>
        /// <param name="dateHours">a <see cref="DateHours"/> instance to add to this instance</param>
        /// <returns></returns>
        public DateHours Add(DateHours dateHours)
        {
            var add = this.TotalHours + dateHours.TotalHours;
            return new DateHours(add);
        }

        /// <summary>
        /// Returns a new <see cref="DateHours"/> with the sum of the hours
        /// </summary>
        /// <param name="hours">hours to add on to this instance</param>
        /// <returns></returns>
        public DateHours Add(double hours)
        {
            return new DateHours(this.TotalHours + hours);
        }
    }
}
