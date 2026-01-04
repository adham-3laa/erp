using EduGate.Models;
using erp.Services;
using System;
using System.Windows;
using System.Windows.Controls;

namespace EduGate.Views.Inventory
{
    public partial class EditProductPage : Page
    {
        private readonly InventoryService _inventoryService;
        private readonly Product _product;

        public EditProductPage(Product product)
        {
            InitializeComponent();

            _inventoryService = new InventoryService();
            _product = product;

            DataContext = _product;
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ===== Parse & Validate Numbers =====
                if (!int.TryParse(SalePriceTextBox.Text, out int salePrice) || salePrice <= 0)
                {
                    MessageBox.Show("سعر البيع غير صالح");
                    return;
                }

                if (!int.TryParse(BuyPriceTextBox.Text, out int buyPrice) || buyPrice <= 0)
                    buyPrice = salePrice;

                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
                    quantity = 1;

                // Assign clean values
                _product.SalePrice = salePrice;
                _product.BuyPrice = buyPrice;
                _product.Quantity = quantity;

                if (string.IsNullOrWhiteSpace(_product.ProductId))
                {
                    MessageBox.Show("ProductId مفقود");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_product.Name))
                {
                    MessageBox.Show("اسم المنتج مطلوب");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_product.Category))
                {
                    MessageBox.Show("CategoryId مطلوب");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_product.SKU))
                    _product.SKU = "N/A";

                await _inventoryService.UpdateProductAsync(_product);

                MessageBox.Show("تم تحديث المنتج بنجاح ✅");
                NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء التحديث: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // يرجّعك للصفحة اللي قبلها (InventoryPage)
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

    }
}
