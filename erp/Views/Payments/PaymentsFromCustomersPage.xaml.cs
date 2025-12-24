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
using erp.ViewModels.Invoices;

namespace erp.Views.Payments
{
    /// <summary>
    /// Interaction logic for PaymentsFromCustomersPage.xaml
    /// </summary>
    public partial class PaymentsFromCustomersPage : Page
    {
        public PaymentsFromCustomersPage()
        {
            InitializeComponent();
            DataContext = new PaymentsFromCustomersViewModel();
        }
    }
}
