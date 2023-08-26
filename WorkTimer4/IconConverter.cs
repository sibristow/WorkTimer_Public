using System;
using System.Globalization;
using System.Windows.Data;

namespace WorkTimer4
{
    /// <summary>
    /// Class to convert from Base-64 encoded PNG to a WPF ImageSource
    /// </summary>
    internal class IconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
                return System.Windows.DependencyProperty.UnsetValue;

            var winImage = Assets.WinFormsAssets.FromEncodedImage(value?.ToString());
            if (winImage is null)
                return System.Windows.DependencyProperty.UnsetValue;

            return Assets.WPFAssets.FromImage(winImage);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
