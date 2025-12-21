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
using erp.DTOS.InvoicesDTOS;

namespace erp.Views.Invoices
{
    public partial class InvoiceDetailsPage : Page
    {
        public InvoiceDetailsPage(InvoiceResponseDto invoice)
        {
            InitializeComponent();
            DataContext = invoice;
        }

        private void Back_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
