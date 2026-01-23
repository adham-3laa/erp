using erp.DTOS.InvoicesDTOS;
using erp.Enums;
using erp.ViewModels.Invoices;
using erp.Views.Payments;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Invoices
{
    public partial class InvoiceDetailsPage : Page
    {
        private readonly InvoiceResponseDto _invoice;
        private readonly InvoiceDetailsViewModel _viewModel;

        public InvoiceDetailsPage(InvoiceResponseDto invoice)
        {
            InitializeComponent();
            _invoice = invoice;
            _viewModel = new InvoiceDetailsViewModel(invoice);
            DataContext = _viewModel;
            
            this.Loaded += InvoiceDetailsPage_Loaded;
        }

        private async void InvoiceDetailsPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel != null)
            {
                await _viewModel.RefreshInvoiceDataAsync();
            }
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void Pay_Click(object sender, RoutedEventArgs e)
        {
            if (_invoice.RemainingAmount <= 0)
                return;

            // ═══════════════════════════════════════════════════════════════
            // SUPPLIER INVOICE & SUPPLIER RETURN INVOICE PAYMENTS
            // ═══════════════════════════════════════════════════════════════
            // - SupplierInvoice: Uses GUID for payment
            // - SupplierReturnInvoice: Uses CODE for payment (critical!)
            // ═══════════════════════════════════════════════════════════════
            
            if (_invoice.InvoiceTypeParsed == InvoiceType.SupplierInvoice || 
                _invoice.InvoiceTypeParsed.IsSupplierReturn())
            {
                NavigationService?.Navigate(
                    new PaySupplierInvoicePage(_invoice));
                return;
            }

            // All other invoice types (Customer, Commission, Return) use order-based payment
            NavigationService?.Navigate(
                new PayInvoiceByOrderPage(_invoice));
        }
    }
}



