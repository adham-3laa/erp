using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace erp.Converters
{
    public class ChequeStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string status = value?.ToString();
            bool isForeground = parameter?.ToString() == "Foreground";

            switch (status)
            {
                case "Collected":
                    return isForeground ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#065F46")) 
                                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1FAE5"));
                case "Rejected":
                    return isForeground ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#991B1B")) 
                                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEE2E2"));
                case "Pending":
                    return isForeground ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#92400E")) 
                                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FEF3C7"));
                case "Cancelled":
                    return isForeground ? new SolidColorBrush((Color)ColorConverter.ConvertFromString("#374151")) 
                                        : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"));
                default:
                    return isForeground ? Brushes.Black : Brushes.Transparent;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class ChequeDirectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isIncoming)
            {
                if (parameter?.ToString() == "Icon")
                    return isIncoming ? "⬇️" : "⬆️";
                return isIncoming ? "وارد (عميل)" : "صادر (مورد)";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }

    public class OverdueToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dueDate && dueDate.Date < DateTime.Today.Date)
            {
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFF1F2")); // Very light red
            }
            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
