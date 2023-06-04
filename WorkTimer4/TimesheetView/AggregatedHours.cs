using System;
using System.Collections.Generic;
using WorkTimer4.API.Data;

namespace WorkTimer4.TimesheetView
{
    /// <summary>
    /// Represents the hours worked on a project aggregated by date
    /// </summary>
    internal class AggregatedHours : Dictionary<string, DateHours>
    {
        internal void AddHours(DateTimeOffset start, double addedHours)
        {
            var dayKey = start.AsAggregationKey();

            if (this.ContainsKey(dayKey))
            {
                // we already have some hours recorded for this activity on this day, so add these
                this[dayKey] += addedHours;
                return;
            }

            // new day for this activity, create new hours
            this[dayKey] = new DateHours(addedHours);
        }
    }
}
