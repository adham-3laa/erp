using System;
using System.Globalization;
using System.Windows.Data;

namespace erp.Converters
{
    public class InitialsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string fullName && !string.IsNullOrWhiteSpace(fullName))
            {
                var names = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (names.Length >= 2)
                {
                    return $"{names[0][0]}{names[1][0]}".ToUpperInvariant();
                }
                else if (names.Length == 1 && names[0].Length > 0)
                {
                    return names[0][0].ToString().ToUpperInvariant();
                }
            }
            return "U"; // Default for "User"
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}