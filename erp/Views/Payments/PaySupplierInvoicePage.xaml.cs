using erp.ViewModels.Invoices;
using System;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace erp.Views.Payments
{
    public partial class PaySupplierInvoicePage : Page
    {
        public PaySupplierInvoicePage(Guid invoiceId, decimal remainingAmount)
        {
            InitializeComponent();
            DataContext = new PaySupplierInvoiceViewModel(invoiceId, remainingAmount);
        }


        private void Back_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var nav = NavigationService.GetNavigationService(this);
            if (nav?.CanGoBack == true)
                nav.GoBack();
        }
    }
}
