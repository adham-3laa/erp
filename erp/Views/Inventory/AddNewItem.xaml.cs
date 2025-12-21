using EduGate.Models;
using EduGate.Services;
using System.Windows;
using System.Windows.Controls;

namespace EduGate.Views.Inventory
{
    public partial class AddNewItem : Page
    {
        private readonly InventoryService _inventoryService = new();

        public AddNewItem()
        {
            InitializeComponent();
            DataContext = new Product();
        }

        private async void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            var product = (Product)DataContext;

            // ✅ Validation
            if (!IsValid(product))
                return;

            try
            {
                await _inventoryService.AddProductAsync(product);

                MessageBox.Show("تم إضافة المنتج بنجاح ✅", "نجاح",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                NavigationService.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    "حدث خطأ أثناء الإضافة:\n" + ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool IsValid(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ShowError("من فضلك أدخل اسم المنتج");
                return false;
            }

            else if (string.IsNullOrWhiteSpace(product.SKU))
            {
                ShowError("من فضلك أدخل SKU");
                return false;
            }

            else if (product.SalePrice <= 0)
            {
                ShowError("سعر البيع لازم يكون أكبر من صفر");
                return false;
            }

            else if (product.BuyPrice <= 0)
            {
                ShowError("سعر الشراء لازم يكون أكبر من صفر");
                return false;
            }

            else if (product.Quantity <= 0)
            {
                ShowError("الكمية لازم تكون أكبر من صفر");
                return false;
            }

            else if (product.Supplier == null)
            {
                ShowError("من فضلك اختر المورد");
                return false;
            }

            else if (product.Category == null)
            {
                ShowError("من فضلك اختر الصنف");
                return false;
            }

            // ❌ الوصف اختياري → مفيش Validation عليه
            return true;
        }

        private void ShowError(string message)
        {
            MessageBox.Show(
                message,
                "تنبيه",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
    }
}
