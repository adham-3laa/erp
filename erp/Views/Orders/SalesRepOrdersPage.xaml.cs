using erp;
using erp.DTOS.Orders;
using erp.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Orders
{
    public partial class SalesRepOrdersPage : Page
    {
        private readonly OrdersService _ordersService;

        public SalesRepOrdersPage()
        {
            InitializeComponent();

            _ordersService = new OrdersService(App.Api);

            Loaded += LoadOrders;

            TopBar.CreateOrderClicked += (_, __) =>
                NavigationService.Navigate(new CreateOrderPage());

            TopBar.ApprovedOrdersClicked += (_, __) =>
                NavigationService.Navigate(new ApprovedOrdersPage());
        }

        private async void LoadOrders(object sender, RoutedEventArgs e)
        {
            try
            {
                OrdersGrid.ItemsSource =
                    await _ordersService.GetConfirmedOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ");
            }
        }

        private async void Approve_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is not OrderDto order)
                return;

            try
            {
                await _ordersService.ApproveOrderAsync(order.id);
                MessageBox.Show("تم اعتماد الطلب ✅");

                LoadOrders(null!, null!);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ");
            }
        }
    }
}
