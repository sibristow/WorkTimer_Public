﻿using System;
using CommunityToolkit.Mvvm.ComponentModel;
using WorkTimer4.API.Data;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    /// <summary>
    /// Represents an entry for the timesheet where the hours worked on a project are aggregated by date
    /// </summary>
    internal class AggregatedTimesheetEntry : ObservableObject
    {
        private string? projectCode;
        private string? activityCode;

        /// <summary>
        /// Gets or sets the project code
        /// </summary>
        public string? ProjectCode
        {
            get
            {
                return projectCode ?? "(undefined)";
            }
            set
            {
                projectCode = value;
                this.OnPropertyChanged(nameof(ProjectCode));
            }
        }

        /// <summary>
        /// Gets or sets the activity code
        /// </summary>
        public string? ActivityCode
        {
            get
            {
                return activityCode ?? "(undefined)";
            }
            set
            {
                activityCode = value;
                this.OnPropertyChanged(nameof(ActivityCode));
            }
        }

        /// <summary>
        /// Gets the activity hours aggregated by date
        /// </summary>
        /// <remarks>
        /// Key is the formatted date, see <see cref="DateTimeExtensions.AsAggregationKey(DateTimeOffset)"/>
        /// </remarks>
        public AggregatedHours AggregatedHours { get; }

        /// <summary>
        /// Gets the activity colour
        /// </summary>
        public string? Colour { get; }


        public AggregatedTimesheetEntry(TimesheetActivity activity)
            : this(activity.ProjectCode, activity.ActivityCode)
        {
            this.Colour = activity.Colour;
        }

        public AggregatedTimesheetEntry(string? projectCode, string? activityCode)
        {
            this.ProjectCode = projectCode;
            this.ActivityCode = activityCode;
            this.AggregatedHours = new AggregatedHours();
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

            this.AggregatedHours.AddHours(start, addedHours);
        }
    }
}
