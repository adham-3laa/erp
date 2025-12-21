using System;
using System.Globalization;
using System.Windows.Data;

namespace erp.Converters
{
    public class StatusTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is bool b && b ? "تعطيل المستخدم" : "تفعيل المستخدم";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
