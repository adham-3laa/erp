using System.Windows;
using System.Windows.Controls;
using AppNavigation = erp.Services.NavigationService;

namespace erp.Views.Reports
{
    public partial class SalesReportPage : Page
    {
        public SalesReportPage()
        {
            InitializeComponent();
            DataContext = new erp.ViewModels.SalesReportViewModel();
        }

        private void OpenStockMovementReport(object sender, RoutedEventArgs e)
            => AppNavigation.NavigateToStockMovementReport();

        private void OpenCommissionReport(object sender, RoutedEventArgs e)
            => AppNavigation.NavigateToCommissionReport();

        private void OpenCustomerReport(object sender, RoutedEventArgs e)
            => AppNavigation.NavigateToCustomerReport();

        private void OpenSalesRepReport(object sender, RoutedEventArgs e)
            => AppNavigation.NavigateToSalesRepReport();

        private void OpenSupplierReport(object sender, RoutedEventArgs e)
            => AppNavigation.NavigateToSupplierReport();
    }
}
