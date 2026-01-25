using System;
using System.Globalization;
using System.Windows.Data;

namespace erp.Converters
{
    public class ChartBarWidthConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 3)
                return 0.0;

            // values[0] = current value (revenue or profit)
            // values[1] = max value in dataset
            // values[2] = container width

            if (values[0] is decimal currentValue && 
                values[1] is decimal maxValue && 
                values[2] is double containerWidth)
            {
                if (maxValue == 0)
                    return 0.0;

                var percentage = (double)(currentValue / maxValue);
                var width = containerWidth * percentage * 0.95; // 95% to leave some margin
                return Math.Max(2, width); // Minimum 2px to be visible
            }

            return 0.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
