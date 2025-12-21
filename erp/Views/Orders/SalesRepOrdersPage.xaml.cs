using EduGate.Views.Orders;
using System.Windows.Controls;

namespace EduGate.Views.Orders
{
    public partial class SalesRepOrdersPage : Page
    {
        public SalesRepOrdersPage()
        {
            InitializeComponent();

            TopBar.CreateOrderClicked += (_, __) =>
                NavigationService.Navigate(new CreateOrderPage());

            TopBar.ApprovedOrdersClicked += (_, __) =>
                NavigationService.Navigate(new ApprovedOrdersPage());
        }
    }
}
