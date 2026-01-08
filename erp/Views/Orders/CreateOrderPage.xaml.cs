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

        // 🔹 مصدر البيانات للـ DataGrid
        private readonly List<CreateOrderItemDto> _items =
            new List<CreateOrderItemDto>();

        public CreateOrderPage()
        {
            InitializeComponent();

            _ordersService = new OrdersService(App.Api);

            // صف افتراضي
            _items.Add(new CreateOrderItemDto());
            ItemsGrid.ItemsSource = _items;

            OrdersTopBarControl.ApprovedOrdersClicked += (_, __) =>
                NavigationService.Navigate(new ApprovedOrdersPage());
        }

        // 🔢 أرقام فقط
        private void NumberOnly(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // ➕ إضافة منتج
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            _items.Add(new CreateOrderItemDto());
            ItemsGrid.Items.Refresh();
        }

        // ❌ حذف منتج
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is CreateOrderItemDto item)
            {
                _items.Remove(item);
                ItemsGrid.Items.Refresh();
            }
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

            var validItems = _items
                .Where(i => !string.IsNullOrWhiteSpace(i.productname) && i.quantity > 0)
                .ToList();

            if (!validItems.Any())
            {
                MessageBox.Show("أدخل منتج واحد على الأقل");
                return;
            }

            var request = new CreateOrderRequestDto
            {
                customername = CustomerNameTextBox.Text.Trim(),
                salesrepname = SalesRepNameTextBox.Text.Trim(),
                items = validItems
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

            _items.Clear();
            _items.Add(new CreateOrderItemDto());
            ItemsGrid.Items.Refresh();
        }
    }
}
