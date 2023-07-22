using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkTimer4.TimesheetView
{
    /// <summary>
    /// Converts total hours to a string showing total hours and reported hours
    /// </summary>
    internal class ReportingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var hours = (double)values[0];
            var reportingFraction = (double)values[1];
            var inv = 1 / reportingFraction;
            var reportedHours = Math.Round(hours * inv, MidpointRounding.ToEven) * reportingFraction;

            return string.Format("{0:F3} / {1:F2}", hours, reportedHours);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
