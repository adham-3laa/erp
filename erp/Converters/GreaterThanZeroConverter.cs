using System;
using System.Globalization;
using System.Windows.Data;

namespace erp.Converters
{
    // 👈 لازم تكون public
    public class GreaterThanZeroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal d)
                return d > 0;

            if (value is double db)
                return db > 0;

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
