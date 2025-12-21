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
                await _inventoryService.UpdateProductAsync(_product);
                MessageBox.Show("تم تحديث المنتج بنجاح ✅");
                NavigationService.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("خطأ أثناء التحديث: " + ex.Message);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoBack();
        }
    }
}
