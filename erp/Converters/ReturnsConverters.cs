using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace erp.Converters
{
    /// <summary>
    /// Converts quantity values for display with Arabic formatting
    /// </summary>
    public class QuantityDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int quantity)
            {
                return quantity.ToString("N0", new CultureInfo("ar-EG"));
            }
            return value?.ToString() ?? "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str && int.TryParse(str, out int result))
                return result;
            return 0;
        }
    }

    /// <summary>
    /// Converts price values for display with Arabic currency formatting
    /// </summary>
    public class PriceDisplayConverter : IValueConverter
    {
        public string CurrencySymbol { get; set; } = "ج.م";

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal price)
            {
                return $"{price:N2} {CurrencySymbol}";
            }
            if (value is double priceDouble)
            {
                return $"{priceDouble:N2} {CurrencySymbol}";
            }
            return value?.ToString() ?? "0.00 " + CurrencySymbol;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Validates quantity input - returns true if quantity is valid (> 0)
    /// </summary>
    public class QuantityValidationConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int quantity)
            {
                return quantity > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Multi-value converter to validate if return quantity is within allowed range
    /// Parameters: [0] = ReturnQuantity, [1] = MaxQuantity
    /// </summary>
    public class ReturnQuantityValidationConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2 &&
                values[0] is int returnQty &&
                values[1] is int maxQty)
            {
                return returnQty > 0 && returnQty <= maxQty;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts validation state to border brush color
    /// </summary>
    public class ValidationToBorderBrushConverter : IValueConverter
    {
        public Brush ValidBrush { get; set; } = new SolidColorBrush(Color.FromRgb(16, 185, 129));  // Green
        public Brush InvalidBrush { get; set; } = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
        public Brush NeutralBrush { get; set; } = new SolidColorBrush(Color.FromRgb(229, 231, 235)); // Gray

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isValid)
            {
                return isValid ? ValidBrush : InvalidBrush;
            }
            return NeutralBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts step number to active/completed status for step indicator
    /// Parameter: step number to check
    /// </summary>
    public class StepStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int currentStep && parameter is string stepParam && int.TryParse(stepParam, out int checkStep))
            {
                if (currentStep > checkStep)
                    return "Completed";
                if (currentStep == checkStep)
                    return "Active";
                return "Pending";
            }
            return "Pending";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts step status to appropriate background brush
    /// </summary>
    public class StepToBrushConverter : IValueConverter
    {
        public Brush ActiveBrush { get; set; } = new SolidColorBrush(Color.FromRgb(79, 70, 229));   // Indigo
        public Brush CompletedBrush { get; set; } = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
        public Brush PendingBrush { get; set; } = new SolidColorBrush(Color.FromRgb(229, 231, 235));  // Gray

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Completed" => CompletedBrush,
                    "Active" => ActiveBrush,
                    _ => PendingBrush
                };
            }
            return PendingBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts step status to foreground (text) color
    /// </summary>
    public class StepToForegroundConverter : IValueConverter
    {
        public Brush ActiveForeground { get; set; } = Brushes.White;
        public Brush CompletedForeground { get; set; } = Brushes.White;
        public Brush PendingForeground { get; set; } = new SolidColorBrush(Color.FromRgb(156, 163, 175)); // Gray-400

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string status)
            {
                return status switch
                {
                    "Completed" => CompletedForeground,
                    "Active" => ActiveForeground,
                    _ => PendingForeground
                };
            }
            return PendingForeground;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Returns opposite of IsCustomerReturn for visibility binding
    /// </summary>
    public class InverseBoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts list count to visibility - visible when count > 0
    /// </summary>
    public class CountToVisibilityConverter : IValueConverter
    {
        public bool InvertResult { get; set; } = false;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool hasItems = false;
            
            if (value is int count)
                hasItems = count > 0;
            else if (value is System.Collections.ICollection collection)
                hasItems = collection.Count > 0;

            if (InvertResult)
                hasItems = !hasItems;

            return hasItems ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Combines multiple boolean values with AND logic for CanExecute scenarios
    /// </summary>
    public class MultiBoolAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (var value in values)
            {
                if (value is bool boolValue && !boolValue)
                    return false;
            }
            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
