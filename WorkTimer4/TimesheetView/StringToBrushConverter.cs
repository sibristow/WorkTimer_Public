using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace WorkTimer4.TimesheetView
{
    internal class StringToBrushConverter : IValueConverter
    {
        private static BrushConverter brushConverter = new BrushConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return Brushes.Black;
            try
            {
                return brushConverter.ConvertFrom(value);
            }
            catch
            {
                return Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
