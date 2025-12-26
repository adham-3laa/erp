using erp.DTOS.InvoicesDTOS;
using erp.ViewModels.Invoices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace erp.Views.Invoices
{
    public partial class InvoicesListPage : Page
    {
        public InvoicesListPage()
        {
            InitializeComponent();
            DataContext = new InvoicesListViewModel();
        }

        // تحميل الفواتير أول ما الصفحة تفتح
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InvoicesListViewModel vm)
                vm.LoadInvoicesCommand.Execute(null);
        }

        // دبل كليك على الفاتورة
        private void InvoicesGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid &&
                grid.SelectedItem is InvoiceResponseDto invoice)
            {
                var nav = NavigationService.GetNavigationService(this);

                if (nav != null)
                {
                    nav.Navigate(new InvoiceDetailsPage(invoice));
                }
                else
                {
                    MessageBox.Show("NavigationService غير متاح – الصفحة ليست داخل Frame");
                }
            }
        }
    }
}
