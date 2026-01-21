using System;
using System.Globalization;
using System.Windows.Data;

namespace erp.Converters
{
    /// <summary>
    /// Converts DateTime to days ago in Arabic
    /// </summary>
    public class DateToDaysAgoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                var days = (DateTime.Now - dateTime).Days;
                if (days == 0) return "اليوم";
                if (days == 1) return "أمس";
                return $"{days} يوم";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts DateTime to Arabic-friendly date format
    /// </summary>
    public class ArabicDateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                return dateTime.ToString("d MMMM yyyy", new CultureInfo("ar-EG"));
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Formats count with Arabic text
    /// </summary>
    public class InvoiceCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return $"{count} فاتورة";
            }
            return "0 فاتورة";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Formats return count with Arabic text
    /// </summary>
    public class ReturnsCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                if (count == 0) return "لا توجد مرتجعات";
                if (count == 1) return "مرتجع واحد";
                if (count == 2) return "مرتجعان";
                return $"{count} مرتجعات";
            }
            return "0 مرتجع";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Calculates age-based color for overdue items
    /// </summary>
    public class DateToAgeColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime dateTime)
            {
                var days = (DateTime.Now - dateTime).Days;
                if (days < 30) return "#10B981"; // Green
                if (days < 60) return "#F59E0B"; // Yellow
                return "#EF4444"; // Red
            }
            return "#6B7280"; // Gray default
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
