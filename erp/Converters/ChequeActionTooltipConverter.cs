using System;
using System.Globalization;
using System.Windows.Data;

namespace erp.Converters
{
    public class ChequeActionTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isIncoming)
            {
                if (parameter?.ToString() == "Accept")
                {
                    return isIncoming ? "تم التحصيل" : "تم الدفع";
                }
                else if (parameter?.ToString() == "Reject")
                {
                    return "رفض / ارتداد";
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}
