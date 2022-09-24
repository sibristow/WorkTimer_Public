using System;
using System.Collections.Generic;
using WorkTimer4.API.Data;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    internal class AggregatedTimesheetEntry
    {
        /// <summary>
        /// Gets or sets the project code
        /// </summary>
        public string? ProjectCode { get; set; }

        /// <summary>
        /// Gets or sets the activity code
        /// </summary>
        public string? ActivityCode { get; set; }

        /// <summary>
        /// Gets the activity hours aggregated by date
        /// </summary>
        /// <remarks>
        /// Key is the formatted date, see <see cref="DateTimeExtensions.AsAggregationKey(DateTimeOffset)"/>
        /// </remarks>
        public Dictionary<string, DateHours> AggregatedHours { get; }


        public AggregatedTimesheetEntry(TimesheetActivity activity)
            : this(activity.ProjectCode, activity.ActivityCode)
        {
        }

        public AggregatedTimesheetEntry(string? projectCode, string? activityCode)
        {
            this.ProjectCode = projectCode;
            this.ActivityCode = activityCode;
            this.AggregatedHours = new Dictionary<string, DateHours>();
        }




        public override bool Equals(object? obj)
        {
            return obj is AggregatedTimesheetEntry data &&
                   this.ProjectCode == data.ProjectCode &&
                   this.ActivityCode == data.ActivityCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.ProjectCode, this.ActivityCode);
        }


        public void AddHours(DateTimeOffset start, DateTimeOffset end)
        {
            if (start.Date != end.Date)
                throw new Exception("Start and End must be the same date.");

            var addedHours = (end - start).TotalHours;

            var dayKey = start.AsAggregationKey();

            if (this.AggregatedHours.ContainsKey(dayKey))
            {
                // we already have some hours recorded for this activity on this day, so add these
                this.AggregatedHours[dayKey] += addedHours;
                return;
            }

            // new day for this activity, create new hours
            this.AggregatedHours[dayKey] = new DateHours(addedHours);
        }
    }
}
