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

            // ✅ استخدام ViewModel مخصص للمرتجعات (Use Enum for safer check)
            if (invoice.InvoiceTypeParsed == erp.Enums.InvoiceType.ReturnInvoice)
            {
                // يمكن إزالة هذا السطر بعد التأكد من العمل
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Loading PayReturnInvoiceViewModel for invoice {invoice.code}");
                DataContext = new PayReturnInvoiceViewModel(invoice);
            }
            else
            {
                DataContext = new PayInvoiceByOrderViewModel(invoice);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var nav = NavigationService.GetNavigationService(this);
            if (nav?.CanGoBack == true)
                nav.GoBack();
        }
    }
}
