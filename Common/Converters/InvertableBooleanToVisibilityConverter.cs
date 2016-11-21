using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Common.Converters
{
    //<UserControl.Resources>
    //  <Converters:InvertableBooleanToVisibilityConverter x:Key="_Converter"/>
    //</UserControl.Resources>
    //<Button Visibility = "{Binding IsRunning, Converter={StaticResource _Converter}, ConverterParameter=Inverted}" > Start </ Button >

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InvertableBooleanToVisibilityConverter : IValueConverter
    {
        enum Parameters
        {
            Normal, Inverted
        }

        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            var boolValue = (bool)value;
            var direction = (Parameters)Enum.Parse(typeof(Parameters), (string)parameter);

            if (direction == Parameters.Inverted)
                return !boolValue ? Visibility.Visible : Visibility.Collapsed;

            return boolValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
