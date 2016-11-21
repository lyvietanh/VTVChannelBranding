using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace Common.Converters
{
    public class BrushGreaterThanMultiValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            Brush result = Brushes.Black;
            try
            {
                if (System.Convert.ToInt32(values[0]) > System.Convert.ToInt32(values[1]))
                {
                    result = Brushes.Red;
                }
            }
            catch (Exception) { }
            return result;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
