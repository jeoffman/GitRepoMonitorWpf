using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GitRepoMonitorWpf.Converters
{
    public class AheadBehindTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int && value != null)
            {
                int status = (int)value;
                SolidColorBrush color;

                if(status > 2)
                    color = new SolidColorBrush(Colors.Red);
                else if (status > 0)
                    color = new SolidColorBrush(Colors.OrangeRed);
                else
                    color = new SolidColorBrush(Colors.Black);
                return color;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
