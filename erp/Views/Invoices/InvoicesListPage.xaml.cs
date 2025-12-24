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
using erp.ViewModels.Invoices;
using erp.Views.Expenses;

namespace erp.Views.Invoices
{
    /// <summary>
    /// Interaction logic for InvoicesListPage.xaml
    /// </summary>
    public partial class InvoicesListPage : Page
    {
        public InvoicesListPage()
        {
            InitializeComponent();
            DataContext = new InvoicesListViewModel();
        }
        private void InvoicesGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid &&
                grid.SelectedItem is InvoiceResponseDto invoice)
            {
                NavigationService.Navigate(new InvoiceDetailsPage(invoice));
            }
        }


    }
}
