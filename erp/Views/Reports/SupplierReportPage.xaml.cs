using erp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Reports
{
    public partial class SupplierReportPage : Page
    {
        public SupplierReportPage()
        {
            InitializeComponent();
            DataContext = new SupplierReportViewModel();
        }

        private void OpenSalesReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new SalesReportPage());
        private void OpenStockMovementReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new StockMovementReportPage());
        private void OpenCommissionReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CommissionReportPage());
        private void OpenCustomerReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new CustomerReportPage());
        private void OpenSalesRepReport(object sender, RoutedEventArgs e) => NavigationService.Navigate(new SalesRepReportPage());
    }
}
