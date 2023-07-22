using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WorkTimer4.API.Data;

namespace WorkTimer4.TimesheetView
{
    /// <summary>
    /// Attached properties and behaviours for generating the aggregation table columns and binding the aggregated hours
    /// </summary>
    internal class DateAggregation
    {
        internal const double DEFAULT_REPORTING = 0.25;
        internal const string HEADER_DATE_FORMAT = "ddd, dd MMM";
        private const int FIXED_COL_COUNT = 3;

        internal static ReportingConverter ReportingConverter = new ReportingConverter();


        public static IEnumerable<DateTimeOffset> GetDateList(DependencyObject obj)
        {
            return (IEnumerable<DateTimeOffset>)obj.GetValue(DateListProperty);
        }

        public static void SetDateList(DependencyObject obj, IEnumerable<DateTimeOffset> value)
        {
            obj.SetValue(DateListProperty, value);
        }

        public static readonly DependencyProperty DateListProperty = DependencyProperty.RegisterAttached("DateList", typeof(IEnumerable<DateTimeOffset>), typeof(DateAggregation), new PropertyMetadata(null, OnDateList_Changed));




        public static double GetReportingFraction(DependencyObject obj)
        {
            return (double)obj.GetValue(ReportingFractionProperty);
        }

        public static void SetReportingFraction(DependencyObject obj, double value)
        {
            obj.SetValue(ReportingFractionProperty, value);
        }

        // Using a DependencyProperty as the backing store for ReportingFraction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ReportingFractionProperty = DependencyProperty.RegisterAttached("ReportingFraction", typeof(double), typeof(DateAggregation), new PropertyMetadata(DEFAULT_REPORTING));


        private static void OnDateList_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid grid)
                return;

            // remove all the previous date columns, but leave the Project and Activity columns
            for (var i = grid.Columns.Count - 1; i > (FIXED_COL_COUNT-1); i--)
            {
                grid.Columns.RemoveAt(i);
            }

            var newDays = e.NewValue as IEnumerable<DateTimeOffset>;

            if (newDays == null)
                return;

            var reportingBinding = new Binding()
            {
               Path = new PropertyPath(ReportingFractionProperty),
               Source = grid,
               FallbackValue = DEFAULT_REPORTING,
               TargetNullValue = DEFAULT_REPORTING
            };

            foreach(var day in newDays.OrderBy(o=>o.UtcDateTime))
            {
                var binding = CreateDayColumnBinding(day, reportingBinding);

                var col = new DataGridTextColumn()
                {
                    Binding = binding,
                    Header = day,
                    HeaderStringFormat = DateAggregation.HEADER_DATE_FORMAT,
                    MinWidth = 100
                };

                grid.Columns.Add(col);
            }
        }

        private static MultiBinding CreateDayColumnBinding(DateTimeOffset day, Binding reportingBinding)
        {
            var dayKey = day.AsAggregationKey();

            var hoursBindingPath = string.Format("{0}[{1}]", nameof(AggregatedTimesheetEntry.AggregatedHours), dayKey);
            var hoursBinding = new Binding(hoursBindingPath)
            {
                TargetNullValue = 0.0,
                FallbackValue = 0.0
            };

            var multiBinding = new MultiBinding()
            {
                Mode = BindingMode.OneWay,
                Converter = ReportingConverter,
                FallbackValue = " - ",
                TargetNullValue = " - "
            };

            multiBinding.Bindings.Add(hoursBinding);
            multiBinding.Bindings.Add(reportingBinding);

            return multiBinding;
        }
    }
}
