using erp.Services;
using erp.DTOS.Inventory.Requests;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Inventory
{
    public partial class StockInProductsPage : Page
    {
        private readonly InventoryService _service = new();

        public StockInProductsPage()
        {
            InitializeComponent();
            ProductsGrid.ItemsSource = new List<StockInItemRequest>();
        }

        private async void Execute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string supplierName = SupplierNameTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(supplierName))
                {
                    MessageBox.Show("اسم المورد مطلوب");
                    return;
                }

                var items = ProductsGrid.ItemsSource
                    .Cast<StockInItemRequest>()
                    .Where(p => !string.IsNullOrWhiteSpace(p.productname) && p.quantity > 0)
                    .ToList();

                if (items.Count == 0)
                {
                    MessageBox.Show("أدخل منتجات صحيحة");
                    return;
                }

                int updatedCount =
                    await _service.StockInProductsAsync(supplierName, items);

                MessageBox.Show($"تم نحديث المنتجات بنجاح");
                NavigationService?.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("خطأ:\n" + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
    }
}
