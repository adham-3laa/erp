using erp.DTOS.InvoicesDTOS;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace erp.Views.Invoices
{
    public partial class InvoiceDetailsPage : Page
    {
        public InvoiceDetailsPage(InvoiceResponseDto invoice)
        {
            InitializeComponent();
            DataContext = invoice;

            // التحكم في زر الدفع
            PayButton.IsEnabled = invoice.RemainingAmount > 0;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var nav = NavigationService.GetNavigationService(this);
            if (nav?.CanGoBack == true)
                nav.GoBack();
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is InvoiceResponseDto invoice)
            {
                if (invoice.RemainingAmount <= 0)
                    return;

                var nav = NavigationService.GetNavigationService(this);
                nav?.Navigate(
                    new erp.Views.Payments.PaySupplierInvoicePage(invoice.Id)
                );
            }
        }
    }
}
