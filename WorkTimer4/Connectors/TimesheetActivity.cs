using System;
using WorkTimer4.API.Data;

namespace WorkTimer4.Connectors
{
    /// <summary>
    /// Activity data serialised by the <see cref="DefaultTimesheetConnector"/>
    /// </summary>
    internal class TimesheetActivity
    {
        /// <summary>
        /// Gets or sets the UTC start time of the activity
        /// </summary>
        public DateTimeOffset Start { get; set; }

        /// <summary>
        /// Gets or sets the UTC end time of the activity
        /// </summary>
        public DateTimeOffset End { get; set; }

        /// <summary>
        /// Gets or sets the project code
        /// </summary>
        public string ProjectCode { get; set; }

        /// <summary>
        /// Gets or sets the activity code
        /// </summary>
        public string ActivityCode { get; set; }

        /// <summary>
        /// Gets or sets the number of hours
        /// </summary>
        public double Hours { get; set; }


        public TimesheetActivity()
        {
        }

        public TimesheetActivity(Activity activity)
        {
            this.Start = activity.Start;
            this.End = activity.End;
            this.ProjectCode = activity.Project.ProjectCode;
            this.ActivityCode = activity.Project.ActivityCode;
            this.Hours = (this.End - this.Start).TotalHours;
        }
    }
}
