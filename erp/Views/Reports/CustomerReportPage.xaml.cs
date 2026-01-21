using erp.ViewModels.Reports;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Reports
{
    public partial class CustomerReportPage : Page
    {
        public CustomerReportPage()
        {
            InitializeComponent();
            DataContext = new CustomerReportViewModel();
        }

        private void OpenSalesReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new SalesReportPage());
        private void OpenStockMovementReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new StockMovementReportPage());
        private void OpenCommissionReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CommissionReportPage());
        private void OpenSalesRepReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new SalesRepReportPage());
        private void OpenSupplierReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new SupplierReportPage());
    }
}
