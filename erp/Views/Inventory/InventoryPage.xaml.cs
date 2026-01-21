using EduGate.Models;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace erp.Views.Inventory
{
    public class EmptyGridMessage
    {
        public string Message { get; set; } = "";
    }

    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public partial class InventoryPage : Page
    {
        private readonly InventoryService _inventoryService;

        // كل المنتجات
        private List<Product> _products = new List<Product>();

        // المصدر الحالي (بحث / كل المنتجات)
        private List<Product> _currentSource = new List<Product>();

        // Pagination
        private int _currentPage = 1;
        private int _itemsPerPage = 10;
        private int _totalPages = 1;

        public InventoryPage()
        {
            InitializeComponent();

            _inventoryService = new InventoryService();
            LoadProducts();

            // TopBar events
            InventoryTopBarControl.AddProductClicked += InventoryTopBar_AddProductClicked;
            InventoryTopBarControl.InventoryCheckClicked += InventoryTopBar_InventoryCheckClicked;
            InventoryTopBarControl.StockInClicked += InventoryTopBar_StockInClicked;
        }

        // ================== TopBar ==================
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
            NavigationService?.Navigate(new StockInProductsPage());
        }

        // ================== Load Products ==================
        private async void LoadProducts()
        {
            try
            {
                _products = await _inventoryService.GetAllProductsAsync();

                _currentSource = _products;
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

        // ================== Pagination Core ==================
        private void LoadProductsPage()
        {
            if (_currentSource == null)
                return;

            if (_currentSource == null || !_currentSource.Any())
            {
                ProductsDataGrid.ItemsSource = null;
                PageTextBlock.Text = "لا توجد بيانات";
                return;
            }

            _totalPages = (int)Math.Ceiling((double)_currentSource.Count / _itemsPerPage);

            if (_currentPage < 1)
                _currentPage = 1;

            if (_currentPage > _totalPages)
                _currentPage = _totalPages;

            var pageItems = _currentSource
                .Skip((_currentPage - 1) * _itemsPerPage)
                .Take(_itemsPerPage)
                .ToList();

            ProductsDataGrid.ItemsSource = pageItems;
            PageTextBlock.Text = $"صفحة {_currentPage} من {_totalPages}";
        }

        // ================== Pagination Buttons ==================
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

        // ================== Refresh ==================
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
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
                    MessageBoxButton.YesNo);

                if (result == MessageBoxResult.Yes)
                {
                    bool success = await _inventoryService.DeleteProductAsync(product.ProductId);

                    if (success)
                        LoadProducts();
                    else
                        MessageBox.Show("حدث خطأ أثناء حذف المنتج");
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

            if (string.IsNullOrWhiteSpace(searchText))
            {
                _currentSource = _products;
                _currentPage = 1;
                LoadProductsPage();
                return;
            }

            try
            {
                var result = await _inventoryService.SearchProductsByNameAsync(searchText);

                if (result == null || result.Count == 0)
                {
                    ShowEmptyMessage("عذرًا، هذا المنتج غير موجود");
                    return;
                }

                _currentSource = result;
                _currentPage = 1;
                LoadProductsPage();
            }
            catch
            {
                ShowEmptyMessage("حدث خطأ أثناء البحث");
            }
        }


        private void ShowEmptyMessage(string message)
        {
            ProductsDataGrid.ItemsSource = new List<EmptyGridMessage>
        {
            new EmptyGridMessage { Message = message }
        };

            PageTextBlock.Text = "";
        }



    }
}
