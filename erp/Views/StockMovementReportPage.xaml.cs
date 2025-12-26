using System.Windows;
using System.Windows.Controls;
using AppNavigation = erp.Services.NavigationService;

namespace erp.Views.Reports
{
    public partial class StockMovementReportPage : Page
    {
        public StockMovementReportPage()
        {
            InitializeComponent();
            DataContext = new erp.ViewModels.Reports.StockMovementReportViewModel();
        }

        private void OpenSalesReport(object sender, RoutedEventArgs e)
            => AppNavigation.NavigateToSalesReport();

        private void OpenCommissionReport(object sender, RoutedEventArgs e)
            => AppNavigation.NavigateToCommissionReport();
    }
}
