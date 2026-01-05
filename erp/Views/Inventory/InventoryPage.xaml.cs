using EduGate.Models;
<<<<<<< HEAD
using EduGate.Services;
using System;
=======
using erp.Services;
>>>>>>> 4e220c83e3af7be7397965530701d15ff1eb99fd
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace EduGate.Views.Inventory
{
    public partial class InventoryPage : Page
    {
        private readonly InventoryService _inventoryService;
        private List<Product> _products;

        private int _currentPage = 1;
        private int _itemsPerPage = 20;
        private int _totalPages = 1;

        public InventoryPage()
        {
            InitializeComponent();

            _inventoryService = new InventoryService();
            LoadProducts();

            // ===== ربط أزرار الـ TopBar =====
            InventoryTopBarControl.AddProductClicked += InventoryTopBar_AddProductClicked;
            InventoryTopBarControl.InventoryCheckClicked += InventoryTopBar_InventoryCheckClicked;

            // 🔴 ده كان ناقص
            InventoryTopBarControl.StockInClicked += InventoryTopBar_StockInClicked;
        }

        // ================== TopBar Handlers ==================
        private void InventoryTopBar_AddProductClicked(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddNewItem());
        }

        private void InventoryTopBar_InventoryCheckClicked(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new InventoryCheckPage());
        }

        private void InventoryTopBar_StockInClicked(object sender, RoutedEventArgs e)
        {
            // صفحة تحديث كمية منتجات
            NavigationService?.Navigate(new StockInProductsPage());
        }

        // ================== تحميل المنتجات ==================
        private async void LoadProducts()
        {
            try
            {
                _products = await _inventoryService.GetAllProductsAsync();
                _currentPage = 1;
                LoadProductsPage();
                ErrorTextBlock.Visibility = Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = ex.Message;
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }

        // ================== Pagination ==================
        private void LoadProductsPage()
        {
            if (_products == null) return;

            _totalPages = (_products.Count + _itemsPerPage - 1) / _itemsPerPage;

            var itemsToShow = _products
                .Skip((_currentPage - 1) * _itemsPerPage)
                .Take(_itemsPerPage)
                .ToList();

            ProductsDataGrid.ItemsSource = itemsToShow;
            PageTextBlock.Text = $"الصفحة {_currentPage} من {_totalPages}";
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        // ================== Delete ==================
        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Product product)
            {
                var result = MessageBox.Show(
                    $"هل تريد حذف المنتج {product.Name}؟",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo
                );

                if (result == MessageBoxResult.Yes)
                {
                    bool success = await _inventoryService.DeleteProductAsync(product.ProductId);
                    if (success)
                        LoadProducts();
                    else
                        MessageBox.Show("حدث خطأ أثناء حذف المنتج.");
                }
            }
        }

        // ================== Edit ==================
        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Product product)
            {
                NavigationService?.Navigate(new EditProductPage(product));
            }
        }

        // ================== Search ==================
        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();

            if (string.IsNullOrEmpty(searchText))
            {
                LoadProducts();
                return;
            }

            try
            {
                var result =
                    await _inventoryService.SearchProductsByNameAsync(searchText);

                ProductsDataGrid.ItemsSource = result;
                PageTextBlock.Text = $"نتائج البحث: {result.Count}";
            }
            catch (Exception ex)
            {
                ErrorTextBlock.Text = ex.Message;
                ErrorTextBlock.Visibility = Visibility.Visible;
            }
        }

        // ================== Pagination Buttons ==================
        private void FirstPage_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = 1;
            LoadProductsPage();
        }

        private void PrevPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage > 1)
            {
                _currentPage--;
                LoadProductsPage();
            }
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            if (_currentPage < _totalPages)
            {
                _currentPage++;
                LoadProductsPage();
            }
        }

        private void LastPage_Click(object sender, RoutedEventArgs e)
        {
            _currentPage = _totalPages;
            LoadProductsPage();
        }
    }
}
