using erp.DTOS.InvoicesDTOS;
using erp.ViewModels.Invoices;
using erp.Views.Payments;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Invoices
{
    public partial class InvoiceDetailsPage : Page
    {
        private readonly InvoiceResponseDto _invoice;

        public InvoiceDetailsPage(InvoiceResponseDto invoice)
        {
            InitializeComponent();
            _invoice = invoice;
            DataContext = new InvoiceDetailsViewModel(invoice);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            if (_invoice.RemainingAmount <= 0)
                return;

            if (_invoice.Type == "SupplierInvoice")
            {
                NavigationService?.Navigate(
                    new PaySupplierInvoicePage(_invoice.Id));
                return;
            }

            NavigationService?.Navigate(
                new PayInvoiceByOrderPage(_invoice));
        }
    }
}
