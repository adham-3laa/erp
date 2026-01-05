using erp;
using erp.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Orders
{
    public partial class ApprovedOrdersPage : Page
    {
        private readonly OrdersService _ordersService;

        public ApprovedOrdersPage()
        {
            InitializeComponent();

            // ✅ ApiClient جاهز وفيه Token
            _ordersService = new OrdersService(App.Api);

            Loaded += LoadApprovedOrders;

            OrdersTopBarControl.CreateOrderClicked += (_, __) =>
                NavigationService.Navigate(new CreateOrderPage());

            OrdersTopBarControl.SalesRepOrdersClicked += (_, __) =>
                NavigationService.Navigate(new SalesRepOrdersPage());
        }

        private async void LoadApprovedOrders(object sender, RoutedEventArgs e)
        {
            try
            {
                OrdersDataGrid.ItemsSource =
                    await _ordersService.GetApprovedOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ أثناء تحميل الطلبات",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
