using System;
using System.Globalization;
using System.Windows.Data;

namespace erp.Converters
{
    public class BoolToTextConverter : IValueConverter
    {
        public string TrueText { get; set; } = "True";
        public string FalseText { get; set; } = "False";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
                return b ? TrueText : FalseText;
            return value?.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
