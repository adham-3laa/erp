using EduGate.Models;
using EduGate.Services;
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
                if (_product.SalePrice <= 0)
                {
                    MessageBox.Show("سعر البيع غير صالح");
                    return;
                }

                if (_product.BuyPrice <= 0)
                    _product.BuyPrice = _product.SalePrice;

                if (_product.Quantity <= 0)
                    _product.Quantity = 1;

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


    }
}
