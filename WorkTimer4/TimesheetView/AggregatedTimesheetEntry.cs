using System;
using System.Collections.Generic;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    internal class AggregatedTimesheetEntry
    {
        public DateTimeOffset Date { get; set; }

        public string ProjectCode { get; set; }

        public string ActivityCode { get; set; }

        public double Hours { get; set; }

        public double QuarterHours
        {
            get
            {
                return Math.Round(this.Hours * 4, MidpointRounding.ToEven) * 0.25;
            }
        }


        public AggregatedTimesheetEntry(TimesheetActivity activity)
            : this(activity.Start, activity.End, activity.ProjectCode, activity.ActivityCode)
        {
        }

        public AggregatedTimesheetEntry(DateTimeOffset start, DateTimeOffset end, string projectCode, string activityCode)
        {
            this.Date = start.Date;
            this.Hours = (end - start).TotalHours;
            this.ProjectCode = projectCode;
            this.ActivityCode = activityCode;
        }
        

        public static bool operator ==(AggregatedTimesheetEntry? left, AggregatedTimesheetEntry? right)
        {
            return EqualityComparer<AggregatedTimesheetEntry>.Default.Equals(left, right);
        }

        public static bool operator !=(AggregatedTimesheetEntry? left, AggregatedTimesheetEntry? right)
        {
            return !(left == right);
        }

        public override bool Equals(object? obj)
        {
            return obj is AggregatedTimesheetEntry entry &&
                   this.Date.Equals(entry.Date) &&
                   this.ProjectCode == entry.ProjectCode &&
                   this.ActivityCode == entry.ActivityCode;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Date, this.ProjectCode, this.ActivityCode);
        }
    }
}
