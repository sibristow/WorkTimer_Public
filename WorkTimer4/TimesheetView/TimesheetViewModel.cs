using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    internal class TimesheetViewModel : ObservableObject
    {
        private DateTime from;
        private DateTime to;
        private DateTimeOffset fromFilter;
        private DateTimeOffset toFilter;
        private IEnumerable<AggregatedTimesheetEntry> timesheetData;
        private readonly List<TimesheetActivity> recorded;

        /// <summary>
        /// Gets the aggregated timesheet data
        /// </summary>
        public IEnumerable<AggregatedTimesheetEntry> TimesheetData
        {
            get
            {
                return timesheetData;
            }
            private set
            {
                timesheetData = value;
                this.OnPropertyChanged(nameof(this.TimesheetData));
            }
        }

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
                this.fromFilter = new DateTimeOffset(this.from.Year, this.from.Month, this.from.Day, 0, 0, 0, TimeSpan.FromHours(0));

                this.OnPropertyChanged(nameof(this.From));

                // refresh the data
                this.TimesheetData = this.AggregateData(this.recorded);

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
                this.toFilter = new DateTimeOffset(this.to.Year, this.to.Month, this.to.Day, 23, 59, 59, 999, TimeSpan.FromHours(0));

                this.OnPropertyChanged(nameof(this.To));
                
                // refresh the data
                this.TimesheetData = this.AggregateData(this.recorded);
            }
        }


        public TimesheetViewModel(List<TimesheetActivity> recorded)
        {
            this.To = DateTime.Today;
            this.From = this.GetPreviousMonday(this.To);
            this.recorded = recorded;

            this.TimesheetData = this.AggregateData(recorded);
        }

        /// <summary>
        /// Gets the date of the previous Monday
        /// </summary>
        /// <param name="fromDate">date to get previous Monday from</param>
        /// <returns></returns>
        private DateTime GetPreviousMonday(DateTime fromDate)
        {
            var daysBack = (int)fromDate.DayOfWeek;
            if (daysBack == (int)DayOfWeek.Sunday)
                daysBack = 7;

            daysBack -= (int)DayOfWeek.Monday;
            daysBack *= -1;

            return fromDate.AddDays(daysBack);
        }



        private IEnumerable<AggregatedTimesheetEntry> AggregateData(List<TimesheetActivity> recorded)
        {
            if (recorded == null)
                return Enumerable.Empty<AggregatedTimesheetEntry>();

            var aggregated = new HashSet<AggregatedTimesheetEntry>();

            foreach (var ts in recorded)
            {
                if (!this.IsInFilter(ts))
                {
                    // outside the filtered range
                    continue;
                }

                if (ts.Start.Date == ts.End.Date)
                {
                    this.AddActivity(aggregated, ts);
                    continue;
                }

                // split activity into separate days



            }

            return aggregated.OrderBy(a => a.Date)
                .ThenBy(a => a.ProjectCode)
                .ThenBy(a => a.ActivityCode)
                .ToList();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="aggregated"></param>
        /// <param name="ts"></param>
        private void AddActivity(HashSet<AggregatedTimesheetEntry> aggregated, TimesheetActivity ts)
        {
            var ag = new AggregatedTimesheetEntry(ts);

            // do we have the date/project/activity combo?
            aggregated.TryGetValue(ag, out var v);

            if (v != null)
            {
                v.Hours += ag.Hours;
                return;
            }

            // new date/project/activity combo
            aggregated.Add(ag);
        }

        /// <summary>
        /// Returns a value indicating whether the activity start date falls within the filtered date range
        /// </summary>
        /// <param name="timesheetActivity"></param>
        /// <returns></returns>
        private bool IsInFilter(TimesheetActivity timesheetActivity)
        {
            return timesheetActivity.Start >= this.fromFilter && timesheetActivity.Start <= this.toFilter;
        }
    }
}
