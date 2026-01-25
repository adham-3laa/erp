using erp.Services;
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

            // التحقق من أن الاسم يتكون من 3 أسماء بالضبط
            var nameParts = CustomerCodeTextBox.Text.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length != 3)
            {
                MessageBox.Show("يجب أن يكون الاسم ثلاثي فقط (الاسم الأول + اسم الأب + اسم الجد)", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                CustomerCodeTextBox.Focus();
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

<<<<<<< HEAD
=======
        private bool ValidateForm()
        {
            var isValid = true;

            // التحقق من اسم العميل
            var customerName = CustomerNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(customerName))
            {
                ShowError(CustomerErrorText, CustomerInputWrapper, "من فضلك أدخل اسم العميل");
                isValid = false;
            }
            else
            {
                // التحقق من أن الاسم ثلاثي (3 كلمات بالضبط)
                var nameParts = customerName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length != 3)
                {
                    ShowError(CustomerErrorText, CustomerInputWrapper, "اسم العميل يجب أن يكون ثلاثياً فقط (لا يقل ولا يزيد عن 3 أسماء)");
                    isValid = false;
                }
            }



            // التحقق من نسبة العمولة
            // التحقق من نسبة العمولة (اختياري)
            var commissionText = CommissionTextBox.Text?.Replace(',', '.') ?? "";
            
            if (!string.IsNullOrWhiteSpace(commissionText))
            {
                if (!double.TryParse(commissionText, NumberStyles.Float, CultureInfo.InvariantCulture, out var commission))
                {
                    ShowError(CommissionErrorText, null, "نسبة العمولة غير صحيحة. يرجى إدخال قيمة رقمية");
                    isValid = false;
                }
                else if (commission < 0 || commission > 100)
                {
                    ShowError(CommissionErrorText, null, "نسبة العمولة يجب أن تكون بين 0 و 100");
                    isValid = false;
                }
            }

            // التحقق من وجود منتج واحد على الأقل
            var validItems = _items
                .Where(i => !string.IsNullOrWhiteSpace(i.productname) && i.quantity > 0)
                .ToList();

            if (validItems.Count == 0)
            {
                ShowError(ProductsErrorText, null, "من فضلك أضف منتجاً واحداً على الأقل مع تحديد الكمية");
                isValid = false;
            }

            return isValid;
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            SetStatus("تم مسح النموذج", StatusType.Info);
        }

>>>>>>> ad1f622d97b67f8b3e45d4015a285558ad57c332
        private void ClearForm()
        {
            CustomerCodeTextBox.Clear();
            ProductNameTextBox.Clear();
            QuantityTextBox.Clear();
        }
    }
}
