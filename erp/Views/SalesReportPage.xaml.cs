using erp.ViewModels;
using System.Windows.Controls;

namespace erp.Views.Reports
{
    public partial class SalesReportPage : Page
    {
        public SalesReportPage()
        {
            InitializeComponent();
            DataContext = new SalesReportViewModel();
        }
    }
}
