using EduGate.Models;
using EduGate.Services;
using System;
using System.Globalization;
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
                if (!int.TryParse(SalePriceTextBox.Text, out int salePrice) || salePrice <= 0)
                {
                    MessageBox.Show("سعر البيع غير صالح");
                    return;
                }

                if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
                {
                    MessageBox.Show("الكمية غير صالحة");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_product.Name))
                {
                    MessageBox.Show("اسم المنتج مطلوب");
                    return;
                }

                if (string.IsNullOrWhiteSpace(_product.Category))
                {
                    MessageBox.Show("اسم الصنف مطلوب");
                    return;
                }

                _product.SalePrice = salePrice;
                _product.Quantity = quantity;

                await _inventoryService.UpdateProductAsync(_product);

                MessageBox.Show("تم تحديث المنتج بنجاح ✅");
                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show("خطأ أثناء التحديث:\n" + ex.Message);
            }
        }


        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
    }
}
