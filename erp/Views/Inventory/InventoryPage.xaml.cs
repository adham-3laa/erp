using EduGate.Models;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private int _itemsPerPage = 12;
        private int _totalPages = 1;

        // Loading state
        private bool _isLoading = false;

        // Search debounce
        private CancellationTokenSource? _searchCts;
        private const int SearchDebounceMs = 400;

        public InventoryPage()
        {
            InitializeComponent();

            _inventoryService = new InventoryService();

            // TopBar events
            InventoryTopBarControl.AddProductClicked += InventoryTopBar_AddProductClicked;
            InventoryTopBarControl.InventoryCheckClicked += InventoryTopBar_InventoryCheckClicked;
            InventoryTopBarControl.StockInClicked += InventoryTopBar_StockInClicked;

            // Load products on initialization
            Loaded += (s, e) => LoadProducts();
        }

        #region TopBar Events

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

        #endregion

        #region Load Products

        private async void LoadProducts()
        {
            if (_isLoading) return;

            try
            {
                SetLoadingState(true);
                HideError();

                _products = await _inventoryService.GetAllProductsAsync();

                _currentSource = _products;
                _currentPage = 1;

                UpdateProductCount();
                LoadProductsPage();

                // Show empty state if no products
                if (_products == null || _products.Count == 0)
                {
                    ShowEmptyState("لا توجد منتجات في المخزون", "ابدأ بإضافة منتجات جديدة للمخزون");
                }
                else
                {
                    HideEmptyState();
                }
            }
            catch (Exception ex)
            {
                ShowError($"حدث خطأ أثناء تحميل المنتجات: {ex.Message}");
                ShowEmptyState("فشل في تحميل البيانات", "تحقق من الاتصال بالإنترنت وحاول مرة أخرى");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        #endregion

        #region Pagination

        private void LoadProductsPage()
        {
            if (_currentSource == null || !_currentSource.Any())
            {
                ProductsDataGrid.ItemsSource = null;
                PageTextBlock.Text = "";
                TotalItemsText.Text = "";
                UpdatePaginationButtons();
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

            // Update pagination UI
            PageTextBlock.Text = $"صفحة {_currentPage} من {_totalPages}";
            TotalItemsText.Text = $"({_currentSource.Count} منتج)";

            UpdatePaginationButtons();
        }

        private void UpdatePaginationButtons()
        {
            PrevButton.IsEnabled = _currentPage > 1;
            NextButton.IsEnabled = _currentPage < _totalPages;
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

        #endregion

        #region Search

        private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = SearchTextBox.Text.Trim();

            // Show/hide clear button
            ClearSearchBtn.Visibility = string.IsNullOrEmpty(searchText) ? Visibility.Collapsed : Visibility.Visible;

            // Cancel previous search
            _searchCts?.Cancel();
            _searchCts = new CancellationTokenSource();
            var token = _searchCts.Token;

            try
            {
                // Debounce
                await Task.Delay(SearchDebounceMs, token);

                if (token.IsCancellationRequested)
                    return;

                if (string.IsNullOrWhiteSpace(searchText))
                {
                    _currentSource = _products;
                    _currentPage = 1;
                    LoadProductsPage();
                    HideEmptyState();
                    UpdateProductCount();
                    return;
                }

                SetLoadingState(true);

                var result = await _inventoryService.SearchProductsByNameAsync(searchText);

                if (token.IsCancellationRequested)
                    return;

                if (result == null || result.Count == 0)
                {
                    _currentSource = new List<Product>();
                    ProductsDataGrid.ItemsSource = null;
                    ShowEmptyState("لا توجد نتائج", $"لم يتم العثور على منتجات تطابق \"{searchText}\"");
                    PageTextBlock.Text = "";
                    TotalItemsText.Text = "";
                    ProductCountText.Text = "0 منتج";
                    return;
                }

                _currentSource = result;
                _currentPage = 1;
                HideEmptyState();
                LoadProductsPage();
                ProductCountText.Text = $"تم العثور على {result.Count} منتج";
            }
            catch (TaskCanceledException)
            {
                // Search was cancelled, ignore
            }
            catch (Exception ex)
            {
                ShowError($"حدث خطأ أثناء البحث: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void ClearSearch_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            SearchTextBox.Focus();
        }

        #endregion

        #region Refresh

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            SearchTextBox.Text = string.Empty;
            LoadProducts();
        }

        #endregion

        #region Delete Product

        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Product product)
            {
                var result = MessageBox.Show(
                    $"هل أنت متأكد من حذف المنتج \"{product.Name}\"؟\n\nهذا الإجراء لا يمكن التراجع عنه.",
                    "تأكيد الحذف",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning,
                    MessageBoxResult.No);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        SetLoadingState(true);
                        HideError();

                        bool success = await _inventoryService.DeleteProductAsync(product.ProductId);

                        if (success)
                        {
                            // Remove from local lists
                            _products.RemoveAll(p => p.ProductId == product.ProductId);
                            _currentSource.RemoveAll(p => p.ProductId == product.ProductId);

                            // Reload page
                            LoadProductsPage();
                            UpdateProductCount();

                            // Show empty state if no products left
                            if (_currentSource.Count == 0)
                            {
                                ShowEmptyState("لا توجد منتجات", "تم حذف جميع المنتجات");
                            }
                        }
                        else
                        {
                            ShowError("فشل في حذف المنتج. يرجى المحاولة مرة أخرى.");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowError($"حدث خطأ أثناء حذف المنتج: {ex.Message}");
                    }
                    finally
                    {
                        SetLoadingState(false);
                    }
                }
            }
        }

        #endregion

        #region Edit Product

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is Product product)
            {
                NavigationService?.Navigate(new EditProductPage(product));
            }
        }

        #endregion

        #region UI State Methods

        private void SetLoadingState(bool isLoading)
        {
            _isLoading = isLoading;
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorBanner.Visibility = Visibility.Visible;
        }

        private void HideError()
        {
            ErrorBanner.Visibility = Visibility.Collapsed;
        }

        private void CloseError_Click(object sender, RoutedEventArgs e)
        {
            HideError();
        }

        private void ShowEmptyState(string title, string subtitle)
        {
            EmptyStateTitle.Text = title;
            EmptyStateSubtitle.Text = subtitle;
            EmptyState.Visibility = Visibility.Visible;
            TableCard.Visibility = Visibility.Collapsed;
        }

        private void HideEmptyState()
        {
            EmptyState.Visibility = Visibility.Collapsed;
            TableCard.Visibility = Visibility.Visible;
        }

        private void UpdateProductCount()
        {
            if (_products == null || _products.Count == 0)
            {
                ProductCountText.Text = "لا توجد منتجات";
            }
            else
            {
                ProductCountText.Text = $"إجمالي {_products.Count} منتج في المخزون";
            }
        }

        #endregion
    }
}
