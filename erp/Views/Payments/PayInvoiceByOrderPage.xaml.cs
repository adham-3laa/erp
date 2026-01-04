using erp.DTOS.InvoicesDTOS;
using erp.ViewModels.Invoices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace erp.Views.Payments
{
    public partial class PayInvoiceByOrderPage : Page
    {
        public PayInvoiceByOrderPage(InvoiceResponseDto invoice)
        {
            InitializeComponent();
            DataContext = new PayInvoiceByOrderViewModel(invoice);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var nav = NavigationService.GetNavigationService(this);
            if (nav?.CanGoBack == true)
                nav.GoBack();
        }
    }
}
