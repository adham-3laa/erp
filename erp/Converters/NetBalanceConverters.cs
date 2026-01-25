using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace erp.Converters
{
    public class NetBalanceTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal balance)
            {
                if (balance < 0) return "(دفع)";
                if (balance > 0) return "(تحصيل)";
                return "(متزن)";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class NetBalanceColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal balance)
            {
                // Negative (We owe him) -> Pay -> Orange/Red
                if (balance < 0) return new SolidColorBrush(Color.FromRgb(251, 146, 60)); // Orange-400
                
                // Positive (He owes us) -> Collect -> Green
                if (balance > 0) return new SolidColorBrush(Color.FromRgb(74, 222, 128)); // Green-400
                
                return Brushes.White;
            }
            return Brushes.White;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
