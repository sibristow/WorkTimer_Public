using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using WorkTimer4.API.Data;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    internal class TimesheetViewModel : ObservableObject
    {
        private DateTime from;
        private DateTime to;
        private DateTimeOffset fromFilter;
        private DateTimeOffset toFilter;

        private IEnumerable<AggregatedTimesheetEntry>? dateTimesheetData;
        private IEnumerable<DateTimeOffset>? dateAggregatedDays;

        private readonly List<TimesheetActivity> recorded;

        /// <summary>
        /// Gets or sets the aggregated timesheet data
        /// </summary>
        public IEnumerable<AggregatedTimesheetEntry>? AggregatedTimesheetData
        {
            get
            {
                return this.dateTimesheetData;
            }
            set
            {
                this.dateTimesheetData = value;
                this.OnPropertyChanged(nameof(this.AggregatedTimesheetData));
            }
        }

        /// <summary>
        /// Gets or sets the list of dates to display
        /// </summary>
        public IEnumerable<DateTimeOffset>? AggregatedDays
        {
            get
            {
                return this.dateAggregatedDays;
            }
            set
            {
                this.dateAggregatedDays = value;
                this.OnPropertyChanged(nameof(this.AggregatedDays));
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
                this.AggregateData(this.recorded);
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
                this.AggregateData(this.recorded);
            }
        }


        public TimesheetViewModel(List<TimesheetActivity> recorded)
        {
            this.To = DateTime.Today;
            this.From = this.To.GetPreviousMonday();
            this.recorded = recorded;

            this.AggregateData(recorded);
        }

        /// <summary>
        /// Aggregates the recorded activity data
        /// </summary>
        /// <param name="recorded"></param>
        private void AggregateData(List<TimesheetActivity> recorded)
        {
            if (recorded == null || !recorded.Any())
            {
                this.dateTimesheetData = Enumerable.Empty<AggregatedTimesheetEntry>();
                this.dateAggregatedDays = Enumerable.Empty<DateTimeOffset>();
                return;
            }

            var dateAggregation = new HashSet<AggregatedTimesheetEntry>();
            var dateList = new HashSet<DateTimeOffset>();

            foreach (var ts in recorded)
            {
                this.AddRecordedActivity(dateAggregation, dateList, ts);
            }

            this.AggregatedTimesheetData = dateAggregation;
            this.AggregatedDays = dateList;
        }

        /// <summary>
        /// Adds the recorded activity to the aggregated data
        /// </summary>
        /// <param name="dateAggregation"></param>
        /// <param name="dateList"></param>
        /// <param name="activity"></param>
        private void AddRecordedActivity(HashSet<AggregatedTimesheetEntry> dateAggregation, HashSet<DateTimeOffset> dateList, TimesheetActivity activity)
        {
            DateTimeOffset hoursFrom = activity.Start;

            while (hoursFrom <= activity.End)
            {
                // determine the end of the current day
                var endOfCurrentDay = hoursFrom.ToEndOfDay();

                // create a new datetime which is the min of the day or the activity end
                var hoursTo = new DateTimeOffset(Math.Min(activity.End.Ticks, endOfCurrentDay.Ticks), hoursFrom.Offset);

                // add the activity hours to the aggregation if it falls within the filter
                this.AggregateIfFiltered(dateAggregation, dateList, activity, hoursFrom, hoursTo);

                // advance the from date to just after the end date
                // (this will either tick over to the next day or put us just after the activity end date)
                hoursFrom = hoursTo.AddMilliseconds(1);
            }
        }

        /// <summary>
        /// Adds the activity's hours to the aggregation if it falls within the filtered date range
        /// </summary>
        /// <param name="dateAggregation"></param>
        /// <param name="dateList"></param>
        /// <param name="ts"></param>
        /// <param name="hoursFrom"></param>
        /// <param name="hoursTo"></param>
        private void AggregateIfFiltered(HashSet<AggregatedTimesheetEntry> dateAggregation, HashSet<DateTimeOffset> dateList, TimesheetActivity ts, DateTimeOffset hoursFrom, DateTimeOffset hoursTo)
        {
            // if the date is within the filter we want to add the hours to the result
            if (!this.IsInFilter(hoursFrom))
                return;

            // we need the start of the day for the date list
            var startOfCurrentDay = hoursFrom.ToStartOfDay();
            dateList.Add(startOfCurrentDay); // hashset so will only add once if the from date is already in the collection

            // create a new aggregated activity which we then see if we have already
            var aggregatedActivity = new AggregatedTimesheetEntry(ts);
            dateAggregation.TryGetValue(aggregatedActivity, out var found);

            if (found != null)
            {
                // already have this activity in the aggregation, so just add this activity's hours
                found.AddHours(hoursFrom, hoursTo);
            }
            else
            {
                // this is a new activity, so set the hours and add it to the dates
                aggregatedActivity.AddHours(hoursFrom, hoursTo);
                dateAggregation.Add(aggregatedActivity);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified date time falls within the filtered date range
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        private bool IsInFilter(DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset >= this.fromFilter && dateTimeOffset <= this.toFilter;
        }
    }
}
