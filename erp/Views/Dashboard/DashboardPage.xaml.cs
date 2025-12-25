using System.Windows.Controls;
using erp.ViewModels.Dashboard;

namespace erp.Views.Dashboard
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            Loaded += async (_, __) =>
            {
                if (DataContext is DashboardViewModel vm)
                    await vm.RefreshAsync();
            };
        }
    }
}
