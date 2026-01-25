using erp.ViewModels;
using System.Windows.Controls;

namespace erp.Views.Reports
{
    public partial class FullSalesReportPage : Page
    {
        public FullSalesReportPage()
        {
            InitializeComponent();
            DataContext = new FullSalesReportViewModel();
        }

        private void OpenStockMovementReport(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService?.Navigate(new StockMovementReportPage());
        }

        private void OpenCommissionReport(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CommissionReportPage());
        }

        private void OpenCustomerReport(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService?.Navigate(new CustomerReportPage());
        }

        private void OpenSalesRepReport(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SalesRepReportPage());
        }

        private void OpenSupplierReport(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService?.Navigate(new SupplierReportPage());
        }

        private void OpenDualRoleReport(object sender, System.Windows.RoutedEventArgs e)
        {
            NavigationService?.Navigate(new DualRoleReportPage());
        }
    }
}
