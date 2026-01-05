using erp.DTOS.Orders;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace erp.Views.Orders
{
    public partial class CreateOrderPage : Page
    {
        private readonly OrdersService _ordersService;

        public CreateOrderPage()
        {
            InitializeComponent();

            _ordersService = new OrdersService(App.Api);

            ItemsGrid.ItemsSource = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto()
            };

            OrdersTopBarControl.ApprovedOrdersClicked += (_, __) =>
                NavigationService.Navigate(new ApprovedOrdersPage());
        }

        private void NumberOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private async void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
            {
                MessageBox.Show("أدخل اسم العميل");
                return;
            }

            if (string.IsNullOrWhiteSpace(SalesRepNameTextBox.Text))
            {
                MessageBox.Show("أدخل اسم المندوب");
                return;
            }

            if (!decimal.TryParse(CommissionTextBox.Text, out var commission))
            {
                MessageBox.Show("أدخل نسبة العمولة");
                return;
            }

            var items = ItemsGrid.Items
                .OfType<CreateOrderItemDto>()
                .Where(i => !string.IsNullOrWhiteSpace(i.productname) && i.quantity > 0)
                .ToList();

            if (!items.Any())
            {
                MessageBox.Show("أدخل منتج واحد على الأقل");
                return;
            }

            var request = new CreateOrderRequestDto
            {
                customername = CustomerNameTextBox.Text.Trim(),
                salesrepname = SalesRepNameTextBox.Text.Trim(),
                items = items
            };

            try
            {
                await _ordersService.CreateOrderAsync(request, commission);
                MessageBox.Show("تم إنشاء الطلب بنجاح ✅");
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ");
            }
        }

        private void ClearForm()
        {
            CustomerNameTextBox.Clear();
            SalesRepNameTextBox.Clear();
            CommissionTextBox.Clear();

            ItemsGrid.ItemsSource = new List<CreateOrderItemDto>
            {
                new CreateOrderItemDto()
            };
        }
    }
}
