using EduGate.Models;
using EduGate.Services;
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

            InventoryTopBarControl.AddProductClicked += InventoryTopBar_AddProductClicked;
        }

        private void InventoryTopBar_AddProductClicked(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddNewItem());
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
            catch
            {
                // ✅ داتا وهمية مؤقتة للتجربة
                _products = GetDummyProducts();
                _currentPage = 1;
                LoadProductsPage();

                ErrorTextBlock.Text = "⚠️ تم تحميل بيانات تجريبية (API غير متصل)";
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
        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (_products == null) return;

            string search = SearchTextBox.Text.Trim();
            if (string.IsNullOrEmpty(search))
            {
                _currentPage = 1;
                LoadProductsPage();
            }
            else
            {
                var filtered = _products
                    .Where(p =>
                        p.ProductId.ToString().Contains(search) ||
                        (p.Name != null && p.Name.Contains(search)) ||
                        (p.Supplier != null && p.Supplier.Contains(search))
                    )
                    .ToList();

                ProductsDataGrid.ItemsSource = filtered;
                PageTextBlock.Text = $"النتيجة {filtered.Count} منتج";
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

        // ================== Dummy Data ==================
        private List<Product> GetDummyProducts()
        {
            return new List<Product>
            {
                new Product
                {
                    ProductId = 1,
                    Name = "لاب توب Dell",
                    Quantity = 15,
                    SalePrice = 18000,
                    BuyPrice = 15000,
                    Supplier = "شركة النور"
                },
                new Product
                {
                    ProductId = 2,
                    Name = "ماوس Logitech",
                    Quantity = 50,
                    SalePrice = 450,
                    BuyPrice = 300,
                    Supplier = "Tech Store"
                },
                new Product
                {
                    ProductId = 3,
                    Name = "كيبورد ميكانيكال",
                    Quantity = 30,
                    SalePrice = 1200,
                    BuyPrice = 850,
                    Supplier = "Gear Hub"
                }
            };
        }
    }
}
