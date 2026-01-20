using erp.ViewModels.Cheques;
using erp.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace erp.Views.Cheques
{
    public partial class ChequesListPage : Page
    {
        private readonly ChequesListViewModel _viewModel;

        public ChequesListPage()
        {
            InitializeComponent();
            _viewModel = new ChequesListViewModel();
            DataContext = _viewModel;
            Loaded += async (s, e) => await _viewModel.LoadChequesAsync();
        }

        private void AddCheque_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new AddChequePage());
        }

        private async void Accept_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is erp.DTOS.Cheques.ChequeDto cheque)
            {
                if (ErrorHandlingService.Confirm($"هل أنت متأكد من تحصيل الشيك رقم {cheque.CheckNumber}؟", "تأكيد التحصيل"))
                {
                    await _viewModel.UpdateStatusAsync(cheque.Code, "Collected");
                }
            }
        }

        private async void Reject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is erp.DTOS.Cheques.ChequeDto cheque)
            {
                if (ErrorHandlingService.Confirm($"هل أنت متأكد من رفض/ارتداد الشيك رقم {cheque.CheckNumber}؟", "تأكيد الرفض"))
                {
                    await _viewModel.UpdateStatusAsync(cheque.Code, "Rejected");
                }
            }
        }
    }

    // Helper Converter logic usually goes in converters folder but putting here for self-containment request
    // or register in resources if separate file.
    // For now simplistic approach is fine, but XAML references BooleanToTextConverter?
    // I need to make sure I add that converter resource or remove it.
    // Let's add it to page resources in XAML or simple inline code.
}

namespace erp.Converters
{
    public class BooleanToTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool b)
                return b ? "وارد" : "صادر";
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
