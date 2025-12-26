using EduGate.Services;
using EduGate.Models;
using erp;
using erp.DTOS.Orders;
using erp.Services;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EduGate.Views.Orders
{
    public partial class CreateOrderPage : Page
    {
        private readonly OrdersService _ordersService;
        private readonly InventoryService _inventoryService;

        // 🔴 GUIDs مؤقتة (من Swagger / DB)
        private const string TEST_SALES_REP_ID =
            "bbbbb-bbbb-bbbb-bbbb-bbbbbbbb";

        private const string TEST_CUSTOMER_ID =
            "ccccc-cccc-cccc-cccc-cccccccc";

        public CreateOrderPage()
        {
            InitializeComponent();

            _ordersService = new OrdersService(App.Api);
            _inventoryService = new InventoryService();

            OrdersTopBarControl.ApprovedOrdersClicked += (_, __) =>
                NavigationService.Navigate(new ApprovedOrdersPage());

            OrdersTopBarControl.SalesRepOrdersClicked += (_, __) =>
                NavigationService.Navigate(new SalesRepOrdersPage());
        }

        // أرقام فقط
        private void Quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private async void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            // ✅ كود العميل (مش اسم)
            if (string.IsNullOrWhiteSpace(CustomerCodeTextBox.Text))
            {
                MessageBox.Show("أدخل كود العميل");
                return;
            }

            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                MessageBox.Show("أدخل اسم المنتج");
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int qty) || qty <= 0)
            {
                MessageBox.Show("أدخل كمية صحيحة");
                return;
            }

            try
            {
                // 🔹 جلب كل المنتجات
                var products = await _inventoryService.GetAllProductsAsync();

                // 🔹 البحث باسم المنتج
                var product = products.FirstOrDefault(p =>
                    p.Name.Equals(ProductNameTextBox.Text.Trim(),
                    StringComparison.OrdinalIgnoreCase));

                if (product == null)
                {
                    MessageBox.Show("المنتج غير موجود في المخزون");
                    return;
                }

                if (qty > product.Quantity)
                {
                    MessageBox.Show($"الكمية المتاحة: {product.Quantity}");
                    return;
                }

                // 🔹 تجهيز الطلب
                var request = new CreateOrderRequestDto
                {
                    // ⚠️ لسه ثابتين مؤقتًا
                    salesrepid = TEST_SALES_REP_ID,
                    customerid = TEST_CUSTOMER_ID,

                    items =
                    {
                        new CreateOrderItemDto
                        {
                            productid = product.ProductId, // GUID حقيقي
                            quantity = qty
                        }
                    }
                };

                await _ordersService.CreateOrderAsync(request);

                MessageBox.Show("تم إنشاء الطلب وتأكيده ✅");
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "خطأ");
            }
        }

        private void ClearForm()
        {
            CustomerCodeTextBox.Clear();
            ProductNameTextBox.Clear();
            QuantityTextBox.Clear();
        }
    }
}
