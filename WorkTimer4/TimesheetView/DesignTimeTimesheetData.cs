#if DEBUG
using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    internal class DesignTimeTimesheetData : ObservableObject//: TimesheetViewModel
    {
        private DateTime from;
        private DateTime to;
        private DateTimeOffset fromFilter;
        private DateTimeOffset toFilter;
        private IEnumerable<TimesheetActivity> recorded;


        /// <summary>
        /// Gets or sets the date to filter the timesheet from
        /// </summary>
        public DateTime From
        {
            get
            {
                return this.from;
            }
            set
            {
                if (value > this.to)
                    value = this.to;

                // set the From date and the UTC offset date used for filtering
                this.from = value;
                this.FromFilter = new DateTimeOffset(this.from.Year, this.from.Month, this.from.Day, 0, 0, 0, TimeSpan.FromHours(0));

                this.OnPropertyChanged(nameof(this.From));
            }
        }

        /// <summary>
        /// Gets or sets the date to filter the timesheet to
        /// </summary>
        public DateTime To
        {
            get
            {
                return this.to;
            }
            set
            {
                if (value < this.from)
                    value = this.from;

                // set the To date and the UTC offset date used for filtering
                this.to = value;
                this.ToFilter = new DateTimeOffset(this.to.Year, this.to.Month, this.to.Day, 23, 59, 59, 999, TimeSpan.FromHours(0));

                this.OnPropertyChanged(nameof(this.To));
            }
        }

        /// <summary>
        /// Gets the date and time to use for filtering activities from
        /// </summary>
        /// <remarks>
        /// This is the start of the day on <see cref="From"/> date
        /// </remarks>
        public DateTimeOffset FromFilter
        {
            get
            {
                return fromFilter;
            }

            private set
            {
                fromFilter = value;
                this.OnPropertyChanged(nameof(FromFilter));
            }
        }

        /// <summary>
        /// Gets the date and time to use for filtering activities to
        /// </summary>
        /// <remarks>
        /// This is the end of the day on <see cref="To"/> date
        /// </remarks>
        public DateTimeOffset ToFilter
        {
            get
            {
                return toFilter;
            }

            private set
            {
                toFilter = value;
                this.OnPropertyChanged(nameof(ToFilter));
            }
        }


        public IEnumerable<TimesheetActivity> Recorded
        {
            get
            {
                return recorded;
            }

            set
            {
                recorded = value;
                this.OnPropertyChanged(nameof(Recorded));
            }
        }

        public DesignTimeTimesheetData()
            //: base()
        {
            var list = new List<TimesheetActivity>();

            // create some dummy items
            for (var i = 0; i< 6; i++)
            {
                var activity = new TimesheetActivity() { ProjectCode = $"Project {i}", ActivityCode = $"Activity {i}" };
                activity.Start = this.From.AddDays(i).AddHours(8).AddMinutes(Random.Shared.Next(180));
                activity.End = activity.Start.AddMinutes(Random.Shared.Next(60));

                var activity2 = new TimesheetActivity() { ProjectCode = $"Project {i}", ActivityCode = $"Activity {i}" };
                activity2.Start = activity.Start.AddMinutes(Random.Shared.Next(120));
                activity2.End = activity2.Start.AddMinutes(Random.Shared.Next(60));

                list.Add(activity);
                list.Add(activity2);
            }

            this.Recorded = list;
        }
    }
}
#endif