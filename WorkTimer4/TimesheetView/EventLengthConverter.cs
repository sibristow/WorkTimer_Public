using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace WorkTimer4.TimesheetView
{

    // https://stackoverflow.com/questions/37949599/how-to-dynamically-draw-a-timeline-in-wpf
    internal class EventStartConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            DateOnly date = (DateOnly)values[0];
            DateTimeOffset start = (DateTimeOffset)values[1];
            double containerWidth = (double)values[2];

            var timelineDuration = 24 * 60 * 60; // whole day
            double factor = start.TimeOfDay.TotalSeconds / timelineDuration;
            double rval = factor * containerWidth;

            if (targetType == typeof(Thickness))
            {
                return new Thickness(rval, 4, 0, 4);
            }
            else
            {
                return rval;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }


    internal class EventWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            DateTimeOffset start = (DateTimeOffset)values[0];
            DateTimeOffset end = (DateTimeOffset)values[1];
            double containerWidth = (double)values[2];

            var timelineDuration = 24 * 60 * 60; // whole day
            var activityDuration = end.TimeOfDay.TotalSeconds - start.TimeOfDay.TotalSeconds;
            double factor = activityDuration / timelineDuration;
            double rval = factor * containerWidth;

            if (targetType == typeof(Thickness))
            {
                return new Thickness(rval, 0, 0, 0);
            }
            else
            {
                return rval;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
