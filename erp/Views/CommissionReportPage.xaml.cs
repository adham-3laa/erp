using System.Windows.Controls;
using AppNavigation = erp.Services.NavigationService;

namespace erp.Views.Reports
{
    public partial class CommissionReportPage : Page
    {
        public CommissionReportPage()
        {
            InitializeComponent();
            DataContext = new erp.ViewModels.Reports.CommissionReportViewModel();
        }

        private void OpenSalesReport(object sender, System.Windows.RoutedEventArgs e)
            => AppNavigation.NavigateToSalesReport();

        private void OpenStockMovementReport(object sender, System.Windows.RoutedEventArgs e)
            => AppNavigation.NavigateToStockMovementReport();
    }
}
