using erp.DTOS.InvoicesDTOS;
using erp.Enums;
using erp.ViewModels.Invoices;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace erp.Views.Payments
{
    public partial class PaySupplierInvoicePage : Page
    {
        /// <summary>
        /// Creates a payment page for Supplier or Supplier Return Invoices.
        /// </summary>
        public PaySupplierInvoicePage(InvoiceResponseDto invoice)
        {
            InitializeComponent();
            DataContext = new PaySupplierInvoiceViewModel(invoice);
        }

        /// <summary>
        /// Backwards-compatible constructor for regular Supplier Invoices.
        /// </summary>
        public PaySupplierInvoicePage(Guid invoiceId, decimal remainingAmount)
        {
             InitializeComponent();
             // Create a temporary DTO wrapper for legacy calls
             var dummyDto = new InvoiceResponseDto 
             { 
                 Id = invoiceId, 
                 RemainingAmount = remainingAmount,
                 Type = "SupplierInvoice" // Default fallback
             };
             DataContext = new PaySupplierInvoiceViewModel(dummyDto);
        }


        private void Back_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var nav = NavigationService.GetNavigationService(this);
            if (nav?.CanGoBack == true)
                nav.GoBack();
        }
    }
}


