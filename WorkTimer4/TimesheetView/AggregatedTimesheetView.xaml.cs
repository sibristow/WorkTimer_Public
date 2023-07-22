using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using WorkTimer4.API.Data;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    /// <summary>
    /// Interaction logic for AggregatedTimesheetView.xaml
    /// </summary>
    public partial class AggregatedTimesheetView : UserControl
    {
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TimesheetActivitiesProperty = DependencyProperty.Register("TimesheetActivities", typeof(IEnumerable<TimesheetActivity>), typeof(AggregatedTimesheetView), new PropertyMetadata(null, OnTimesheetActivities_Changed));

        // Using a DependencyProperty as the backing store for FromFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FromFilterProperty = DependencyProperty.Register("FromFilter", typeof(DateTimeOffset), typeof(AggregatedTimesheetView), new PropertyMetadata(default(DateTimeOffset), OnFilter_Changed));

        // Using a DependencyProperty as the backing store for FromFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToFilterProperty = DependencyProperty.Register("ToFilter", typeof(DateTimeOffset), typeof(AggregatedTimesheetView), new PropertyMetadata(default(DateTimeOffset), OnFilter_Changed));

        // Using a DependencyProperty as the backing store for RecordingQuantity.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReportingFractionProperty = DependencyProperty.Register("ReportingFraction", typeof(double), typeof(AggregatedTimesheetView), new PropertyMetadata(0.25));

        // Using a DependencyProperty as the backing store for AggregatedTimesheetData.  This enables animation, styling, binding, etc...
        private static readonly DependencyPropertyKey AggregatedTimesheetDataPropertyKey = DependencyProperty.RegisterReadOnly("AggregatedTimesheetData", typeof(IEnumerable<AggregatedTimesheetEntry>), typeof(AggregatedTimesheetView), new PropertyMetadata(null));

        // Using a DependencyProperty as the backing store for AggregatedTimesheetData.  This enables animation, styling, binding, etc...
        private static readonly DependencyPropertyKey AggregatedDaysPropertyKey = DependencyProperty.RegisterReadOnly("AggregatedDays", typeof(IEnumerable<DateTimeOffset>), typeof(AggregatedTimesheetView), new PropertyMetadata(null));



        public double ReportingFraction
        {
            get { return (double)GetValue(ReportingFractionProperty); }
            set { SetValue(ReportingFractionProperty, value); }
        }

        public DateTimeOffset FromFilter
        {
            get { return (DateTimeOffset)GetValue(FromFilterProperty); }
            set { SetValue(FromFilterProperty, value); }
        }

        public DateTimeOffset ToFilter
        {
            get { return (DateTimeOffset)GetValue(ToFilterProperty); }
            set { SetValue(ToFilterProperty, value); }
        }


        /// <summary>
        /// Gets or sets the timesheet activities to populate the table with
        /// </summary>
        internal IEnumerable<TimesheetActivity> TimesheetActivities
        {
            get { return (IEnumerable<TimesheetActivity>)GetValue(TimesheetActivitiesProperty); }
            set { SetValue(TimesheetActivitiesProperty, value); }
        }

        /// <summary>
        /// Gets the aggregated timesheet data
        /// </summary>
        internal IEnumerable<AggregatedTimesheetEntry>? AggregatedTimesheetData
        {
            get
            {
                return (IEnumerable<AggregatedTimesheetEntry>)GetValue(AggregatedTimesheetDataPropertyKey.DependencyProperty);
            }
            private set
            {
                SetValue(AggregatedTimesheetDataPropertyKey, value);
            }
        }

        /// <summary>
        /// Gets the list of dates to display
        /// </summary>
        internal IEnumerable<DateTimeOffset>? AggregatedDays
        {
            get
            {
                return (IEnumerable<DateTimeOffset>)GetValue(AggregatedDaysPropertyKey.DependencyProperty);
            }
            private set
            {
                SetValue(AggregatedDaysPropertyKey, value);
            }
        }




        public AggregatedTimesheetView()
        {
            InitializeComponent();
        }



        private static void OnTimesheetActivities_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AggregatedTimesheetView view)
                return;

            view.AggregateData();
        }

        private static void OnFilter_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not AggregatedTimesheetView view)
                return;

            view.AggregateData();
        }


        /// <summary>
        /// Aggregates the recorded activity data
        /// </summary>
        /// <param name="recorded"></param>
        private void AggregateData()
        {
            if (this.TimesheetActivities == null || !this.TimesheetActivities.Any())
            {
                this.AggregatedTimesheetData = Enumerable.Empty<AggregatedTimesheetEntry>();
                this.AggregatedDays = Enumerable.Empty<DateTimeOffset>();
                return;
            }

            var dateAggregation = new HashSet<AggregatedTimesheetEntry>();
            var dateList = new HashSet<DateTimeOffset>();

            foreach (var ts in this.TimesheetActivities)
            {
                this.AddRecordedActivity(dateAggregation, dateList, ts);
            }

            this.AddTotal(dateAggregation);

            this.AggregatedTimesheetData = dateAggregation;
            this.AggregatedDays = dateList;
        }

        /// <summary>
        /// Adds a summary total row to the aggregated data collection
        /// </summary>
        /// <param name="dateAggregation"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void AddTotal(HashSet<AggregatedTimesheetEntry> dateAggregation)
        {
            var row = new AggregatedTimesheetEntry("Summary", "Total") { IsSummaryRow = true };
            foreach(var entry in dateAggregation)
            {
                foreach(var hours in entry.AggregatedHours)
                {
                    if (!row.AggregatedHours.ContainsKey(hours.Key))
                    {
                        row.AggregatedHours.Add(hours.Key, 0);
                    }

                    row.AggregatedHours[hours.Key] += hours.Value;
                }
            }

            dateAggregation.Add(row);
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
        /// <param name="activity"></param>
        /// <param name="hoursFrom"></param>
        /// <param name="hoursTo"></param>
        private void AggregateIfFiltered(HashSet<AggregatedTimesheetEntry> dateAggregation, HashSet<DateTimeOffset> dateList, TimesheetActivity activity, DateTimeOffset hoursFrom, DateTimeOffset hoursTo)
        {
            // if the date is within the filter we want to add the hours to the result
            if (!this.IsInFilter(hoursFrom))
                return;

            // we need the start of the day for the date list
            var startOfCurrentDay = hoursFrom.ToStartOfDay();
            dateList.Add(startOfCurrentDay); // hashset so will only add once if the from date is already in the collection

            // create a new aggregated activity which we then see if we have already
            var aggregatedActivity = new AggregatedTimesheetEntry(activity);
            dateAggregation.TryGetValue(aggregatedActivity, out var found);

            if (found is not null)
            {
                // already have this activity in the aggregation, so just add this activity's hours
                found.AddHours(hoursFrom, hoursTo, activity);
            }
            else
            {
                // this is a new activity, so set the hours and add it to the dates
                aggregatedActivity.AddHours(hoursFrom, hoursTo, activity);
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
            return dateTimeOffset >= this.FromFilter && dateTimeOffset <= this.ToFilter;
        }
    }
}
