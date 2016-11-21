using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Common.Converters
{
    [ValueConversion(typeof(DateTime), typeof(int))]
    public class DateOffsetByTodayValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime date = (DateTime)value;
            TimeSpan comparison = date.Date - DateTime.Now.Date;
            return (int)comparison.TotalDays;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
