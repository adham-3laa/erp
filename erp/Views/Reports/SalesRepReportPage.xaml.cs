using erp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Reports
{
    public partial class SalesRepReportPage : Page
    {
        public SalesRepReportPage()
        {
            InitializeComponent();
            DataContext = new SalesRepReportViewModel();
        }

        private void OpenSalesReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new FullSalesReportPage());
        private void OpenStockMovementReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new StockMovementReportPage());
        private void OpenCommissionReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CommissionReportPage());
        private void OpenCustomerReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CustomerReportPage());
        private void OpenSupplierReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new SupplierReportPage());
        private void OpenDualRoleReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new DualRoleReportPage());
    }
}
