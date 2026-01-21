using System;
using System.Globalization;
using System.Windows.Data;

namespace erp.Converters
{
    /// <summary>
    /// Converter to check if a quantity value is less than a specified threshold.
    /// Used for UI urgency hierarchy in stock quantity displays.
    /// </summary>
    public class QuantityLessThanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return false;

            try
            {
                // Convert the current quantity
                int quantity = System.Convert.ToInt32(value);
                
                // Convert the threshold parameter
                int threshold = System.Convert.ToInt32(parameter);
                
                // Return true if quantity is less than threshold AND greater than 0
                // (0 is handled separately as critical state)
                return quantity > 0 && quantity < threshold;
            }
            catch
            {
                return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
