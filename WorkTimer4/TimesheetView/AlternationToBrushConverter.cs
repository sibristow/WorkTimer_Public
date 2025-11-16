using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace WorkTimer4.TimesheetView
{
    internal class AlternationToBrushConverter : DependencyObject, IValueConverter
    {
        public static readonly DependencyProperty EvenBrushProperty = DependencyProperty.Register(nameof(EvenBrush), typeof(Brush), typeof(AlternationToBrushConverter), new PropertyMetadata(Brushes.White));
        public static readonly DependencyProperty OddBrushProperty = DependencyProperty.Register(nameof(OddBrush), typeof(Brush), typeof(AlternationToBrushConverter), new PropertyMetadata(Brushes.LightGray));

        public Brush EvenBrush 
        {
            get
            {
                return (Brush)this.GetValue(EvenBrushProperty);
            }
            set
            {
                this.SetValue(EvenBrushProperty, value);
            }
        }
        
        public Brush OddBrush
        {
            get
            {
                return (Brush)this.GetValue(OddBrushProperty);
            }
            set
            {
                this.SetValue(OddBrushProperty, value);
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = (int)value;
            return index % 2 == 0 ? EvenBrush : OddBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

    }
}
