using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using WorkTimer4.API.Data;

namespace WorkTimer4.TimesheetView
{
    internal class DateAggregation
    {
        internal const string HEADER_DATE_FORMAT = "ddd, dd MMM";

        private const int FIXED_COL_COUNT = 3;

        public static IEnumerable<DateTimeOffset> GetDateList(DependencyObject obj)
        {
            return (IEnumerable<DateTimeOffset>)obj.GetValue(DateListProperty);
        }

        public static void SetDateList(DependencyObject obj, IEnumerable<DateTimeOffset> value)
        {
            obj.SetValue(DateListProperty, value);
        }

        public static readonly DependencyProperty DateListProperty = DependencyProperty.RegisterAttached("DateList", typeof(IEnumerable<DateTimeOffset>), typeof(DateAggregation), new PropertyMetadata(null, OnDateList_Changed));



        private static void OnDateList_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as DataGrid;
            if (grid == null)
                return;

            // remove all the previous date columns, but leave the Project and Activity columns
            for (var i = grid.Columns.Count - 1; i > (FIXED_COL_COUNT-1); i--)
            {
                grid.Columns.RemoveAt(i);
            }

            var newDays = e.NewValue as IEnumerable<DateTimeOffset>;

            if (newDays == null)
                return;

            foreach(var day in newDays.OrderBy(o=>o.UtcDateTime))
            {
                var binding = CreateDayColumnBinding(day);

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

        private static Binding CreateDayColumnBinding(DateTimeOffset day)
        {
            var dayKey = day.AsAggregationKey();

            var bindingPath = string.Format("{0}[{1}].{2}", nameof(AggregatedTimesheetEntry.AggregatedHours), dayKey, nameof(DateHours.FormattedHours));

            return new Binding(bindingPath)
            {
                //StringFormat = "F3",
                TargetNullValue = string.Empty,
                FallbackValue = string.Empty
            };
        }
    }
}
