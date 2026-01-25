using erp.Services;
using erp.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Reports
{
    public partial class DualRoleReportPage : Page
    {
        public DualRoleReportPage()
        {
            InitializeComponent();
            DataContext = new DualRoleReportViewModel();
        }

        private void OpenSalesReport(object sender, RoutedEventArgs e) 
            => NavigationService.Navigate(new FullSalesReportPage());
            
        private void OpenStockMovementReport(object sender, RoutedEventArgs e) 
            => NavigationService.Navigate(new StockMovementReportPage());
            
        private void OpenCommissionReport(object sender, RoutedEventArgs e) 
            => NavigationService.Navigate(new CommissionReportPage());
            
        private void OpenCustomerReport(object sender, RoutedEventArgs e) 
            => NavigationService.Navigate(new CustomerReportPage());
            
        private void OpenSalesRepReport(object sender, RoutedEventArgs e) 
            => NavigationService.Navigate(new SalesRepReportPage());
            
        private void OpenSupplierReport(object sender, RoutedEventArgs e) 
            => NavigationService.Navigate(new SupplierReportPage());
    }
}
