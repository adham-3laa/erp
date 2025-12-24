using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using erp.Views.Invoices;

namespace erp.Views.Payments
{
    /// <summary>
    /// Interaction logic for PaymentsMenuPage.xaml
    /// </summary>
    public partial class PaymentsMenuPage : Page
    {
        public PaymentsMenuPage()
        {
            InitializeComponent();
        }

        private void CustomersPayments_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainFrame
                .Navigate(new PaymentsFromCustomersPage());
        }

        private void SuppliersPayments_Click(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow)
                .MainFrame
                .Navigate(new Views.Payments.PaySupplierPage());
        }
    }
}
