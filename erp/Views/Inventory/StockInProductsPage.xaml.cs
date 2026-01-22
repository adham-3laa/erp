using erp.Services;
using erp.DTOS.Inventory.Requests;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace erp.Views.Inventory
{
    public partial class StockInProductsPage : Page
    {
        private readonly InventoryService _inventoryService = new();
        private readonly OrdersService _ordersService;

        // مصدر البيانات للمنتجات
        private readonly ObservableCollection<StockInItemViewModel> _products = new();

        // Autocomplete suggestions
        private List<SupplierAutocompleteItem> _supplierSuggestions = new();
        private List<ProductAutocompleteItem> _productSuggestions = new();

        // Debounce timers
        private CancellationTokenSource? _supplierSearchCts;
        private CancellationTokenSource? _productSearchCts;

        // المورد المحدد
        private SupplierAutocompleteItem? _selectedSupplier;

        // المنتج الحالي للـ Autocomplete
        private TextBox? _currentProductTextBox;
        private StockInItemViewModel? _currentProductItem;

        // ثابت لوقت التأخير في البحث (بالمللي ثانية)
        private const int SearchDebounceMs = 300;

        public StockInProductsPage()
        {
            InitializeComponent();
            _ordersService = new OrdersService(App.Api);

            // إضافة منتج افتراضي
            _products.Add(new StockInItemViewModel());
            ProductsItemsControl.ItemsSource = _products;
            UpdateItemsCount();

            UpdatePlaceholders();
        }

        #region === Placeholders ===

        private void UpdatePlaceholders()
        {
            if (SupplierPlaceholder != null)
                SupplierPlaceholder.Visibility = string.IsNullOrEmpty(SupplierNameTextBox?.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region === Supplier Autocomplete ===

        private async void SupplierNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholders();
            ClearError(SupplierErrorText, SupplierInputWrapper);

            var searchText = SupplierNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                SupplierSuggestionsPopup.IsOpen = false;
                _selectedSupplier = null;
                return;
            }

            // إلغاء البحث السابق
            _supplierSearchCts?.Cancel();
            _supplierSearchCts = new CancellationTokenSource();
            var token = _supplierSearchCts.Token;

            try
            {
                ShowSupplierLoading(true);
                SupplierSuggestionsPopup.IsOpen = true;

                await Task.Delay(SearchDebounceMs, token);
                if (token.IsCancellationRequested) return;

                // البحث من الـ API
                _supplierSuggestions = await _ordersService.GetSuppliersAutocompleteAsync(searchText);

                if (token.IsCancellationRequested) return;

                ShowSupplierLoading(false);

                if (_supplierSuggestions.Count > 0)
                {
                    SupplierSuggestionsListBox.ItemsSource = _supplierSuggestions;
                    SupplierSuggestionsListBox.Visibility = Visibility.Visible;
                    SupplierNoResultsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    SupplierSuggestionsListBox.Visibility = Visibility.Collapsed;
                    SupplierNoResultsText.Visibility = Visibility.Visible;
                }
            }
            catch (OperationCanceledException) { }
            catch
            {
                ShowSupplierLoading(false);
                SupplierNoResultsText.Text = "خطأ في البحث";
                SupplierNoResultsText.Visibility = Visibility.Visible;
                SupplierSuggestionsListBox.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowSupplierLoading(bool show)
        {
            SupplierLoadingText.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            SupplierSuggestionsListBox.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            SupplierNoResultsText.Visibility = Visibility.Collapsed;
        }

        private void SupplierNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SupplierNameTextBox.Text) && _supplierSuggestions.Count > 0)
            {
                SupplierSuggestionsPopup.IsOpen = true;
            }
        }

        private void SupplierNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Task.Delay(200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!SupplierSuggestionsListBox.IsMouseOver && !SupplierNameTextBox.IsFocused)
                    {
                        SupplierSuggestionsPopup.IsOpen = false;
                    }
                });
            });
        }

        private void SupplierNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!SupplierSuggestionsPopup.IsOpen) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (SupplierSuggestionsListBox.Items.Count > 0)
                    {
                        SupplierSuggestionsListBox.Focus();
                        SupplierSuggestionsListBox.SelectedIndex = Math.Min(
                            SupplierSuggestionsListBox.SelectedIndex + 1,
                            SupplierSuggestionsListBox.Items.Count - 1);
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (SupplierSuggestionsListBox.Items.Count > 0)
                    {
                        SupplierSuggestionsListBox.Focus();
                        SupplierSuggestionsListBox.SelectedIndex = Math.Max(
                            SupplierSuggestionsListBox.SelectedIndex - 1, 0);
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (SupplierSuggestionsListBox.SelectedItem is SupplierAutocompleteItem selected)
                    {
                        SelectSupplier(selected);
                    }
                    e.Handled = true;
                    break;

                case Key.Escape:
                    SupplierSuggestionsPopup.IsOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        private void SupplierSuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void SupplierSuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SupplierSuggestionsListBox.SelectedItem is SupplierAutocompleteItem selected)
            {
                SelectSupplier(selected);
            }
        }

        private void SupplierSuggestionsListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void SelectSupplier(SupplierAutocompleteItem supplier)
        {
            _selectedSupplier = supplier;
            SupplierNameTextBox.Text = supplier.fullname;
            SupplierSuggestionsPopup.IsOpen = false;
            SupplierNameTextBox.Focus();
            SupplierNameTextBox.CaretIndex = SupplierNameTextBox.Text.Length;
            UpdatePlaceholders();
        }

        #endregion

        #region === Product Autocomplete ===

        private async void ProductNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            if (textBox.Tag is not StockInItemViewModel item) return;

            _currentProductTextBox = textBox;
            _currentProductItem = item;

            // إعادة تعيين حالة التحقق
            item.IsValid = null;

            var searchText = textBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                ProductSuggestionsPopup.IsOpen = false;
                return;
            }

            // إلغاء البحث السابق
            _productSearchCts?.Cancel();
            _productSearchCts = new CancellationTokenSource();
            var token = _productSearchCts.Token;

            try
            {
                ProductSuggestionsPopup.PlacementTarget = textBox;
                ShowProductLoading(true);
                ProductSuggestionsPopup.IsOpen = true;

                await Task.Delay(SearchDebounceMs, token);
                if (token.IsCancellationRequested) return;

                // البحث من الـ API
                _productSuggestions = await _ordersService.GetProductsAutocompleteAsync(searchText);

                if (token.IsCancellationRequested) return;

                ShowProductLoading(false);

                if (_productSuggestions.Count > 0)
                {
                    ProductSuggestionsListBox.ItemsSource = _productSuggestions;
                    ProductSuggestionsListBox.Visibility = Visibility.Visible;
                    ProductNoResultsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ProductSuggestionsListBox.Visibility = Visibility.Collapsed;
                    ProductNoResultsText.Visibility = Visibility.Visible;
                    item.IsValid = false;
                }
            }
            catch (OperationCanceledException) { }
            catch
            {
                ShowProductLoading(false);
                ProductNoResultsText.Text = "خطأ في البحث";
                ProductNoResultsText.Visibility = Visibility.Visible;
                ProductSuggestionsListBox.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowProductLoading(bool show)
        {
            ProductLoadingText.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            ProductSuggestionsListBox.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            ProductNoResultsText.Visibility = Visibility.Collapsed;
        }

        private void ProductNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Tag is StockInItemViewModel item)
            {
                _currentProductTextBox = textBox;
                _currentProductItem = item;
            }
        }

        private void ProductNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Task.Delay(200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!ProductSuggestionsListBox.IsMouseOver)
                    {
                        ProductSuggestionsPopup.IsOpen = false;
                    }
                });
            });
        }

        private void ProductSuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductSuggestionsListBox.SelectedItem is ProductAutocompleteItem selected)
            {
                if (_currentProductItem != null)
                {
                    _currentProductItem.productname = selected.name;
                    _currentProductItem.IsValid = true;
                }

                if (_currentProductTextBox != null)
                {
                    _currentProductTextBox.Text = selected.name;
                    _currentProductTextBox.CaretIndex = _currentProductTextBox.Text.Length;
                }

                ProductSuggestionsPopup.IsOpen = false;
            }
        }

        #endregion

        #region === Products Management ===

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            _products.Add(new StockInItemViewModel());
            UpdateItemsCount();
            ClearError(ProductsErrorText, null);
        }

        private void RemoveProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is StockInItemViewModel item)
            {
                if (_products.Count > 1)
                {
                    _products.Remove(item);
                    UpdateItemsCount();
                }
                else
                {
                    // لا يمكن حذف آخر عنصر - مسح البيانات فقط
                    item.productname = "";
                    item.quantity = 1;
                    item.IsValid = null;
                }
            }
        }

        private void UpdateItemsCount()
        {
            var validCount = _products.Count(p => !string.IsNullOrWhiteSpace(p.productname) && p.quantity > 0);
            ItemsCountBadge.Text = validCount.ToString();
        }

        #endregion

        #region === Input Validation ===

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        #endregion

        #region === Execute ===

        private async void Execute_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();

            // التحقق من المورد
            if (string.IsNullOrWhiteSpace(SupplierNameTextBox.Text))
            {
                ShowError(SupplierErrorText, SupplierInputWrapper, "من فضلك أدخل اسم المورد");
                SupplierNameTextBox.Focus();
                return;
            }

            // التحقق من أن المورد تم اختياره من القائمة
            var supplierName = SupplierNameTextBox.Text.Trim();
            if (_selectedSupplier == null || _selectedSupplier.fullname.Trim() != supplierName)
            {
                ShowError(SupplierErrorText, SupplierInputWrapper, "يجب اختيار المورد من القائمة المنسدلة");
                SupplierNameTextBox.Focus();
                return;
            }

            // التحقق من المنتجات
            var validProducts = _products
                .Where(p => !string.IsNullOrWhiteSpace(p.productname) && p.quantity > 0)
                .ToList();

            if (validProducts.Count == 0)
            {
                ShowError(ProductsErrorText, null, "من فضلك أضف منتجاً واحداً على الأقل مع تحديد الكمية");
                return;
            }

            // التحقق من أن جميع المنتجات تم اختيارها من القائمة (موجودة في النظام)
            var invalidProducts = validProducts.Where(p => p.IsValid == false || p.IsValid == null).ToList();
            if (invalidProducts.Count > 0)
            {
                var invalidNames = string.Join("، ", invalidProducts.Select(p => p.productname));
                ShowError(ProductsErrorText, null, $"المنتجات التالية غير موجودة في النظام: {invalidNames}\nيرجى اختيار المنتجات من القائمة المنسدلة");
                return;
            }

            try
            {
                SetLoading(true);
                SetStatus("جاري تحديث المخزون...", StatusType.Loading);

                // تحويل البيانات
                var items = validProducts.Select(p => new StockInItemRequest
                {
                    productname = p.productname.Trim(),
                    quantity = p.quantity
                }).ToList();

                // إرسال للـ API
                await _inventoryService.StockInProductsAsync(supplierName, items);

                SetLoading(false);
                SetStatus("تم تحديث المخزون بنجاح! ✅", StatusType.Success);

                MessageBox.Show(
                    $"تم تحديث كمية {validProducts.Count} منتج بنجاح ✅",
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                SetLoading(false);
                SetStatus("فشل في تحديث المخزون", StatusType.Error);

                MessageBox.Show(
                    $"حدث خطأ أثناء تحديث المخزون:\n{ex.Message}",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            SupplierNameTextBox.Clear();
            _selectedSupplier = null;

            _products.Clear();
            _products.Add(new StockInItemViewModel());
            UpdateItemsCount();
            UpdatePlaceholders();
            ClearAllErrors();
            SetStatus("تم مسح النموذج", StatusType.Info);
        }

        #endregion

        #region === Error Handling ===

        private void ShowError(TextBlock errorTextBlock, Border? inputWrapper, string message)
        {
            errorTextBlock.Text = message;
            errorTextBlock.Visibility = Visibility.Visible;

            if (inputWrapper != null)
            {
                inputWrapper.Background = new LinearGradientBrush(
                    Color.FromRgb(254, 202, 202),
                    Color.FromRgb(252, 165, 165),
                    45);
            }
        }

        private void ClearError(TextBlock errorTextBlock, Border? inputWrapper)
        {
            errorTextBlock.Visibility = Visibility.Collapsed;

            if (inputWrapper != null)
            {
                inputWrapper.Background = new LinearGradientBrush(
                    Color.FromRgb(229, 231, 235),
                    Color.FromRgb(209, 213, 219),
                    45);
            }
        }

        private void ClearAllErrors()
        {
            ClearError(SupplierErrorText, SupplierInputWrapper);
            ClearError(ProductsErrorText, null);
        }

        #endregion

        #region === UI Helpers ===

        private enum StatusType { Info, Loading, Success, Error }

        private void SetStatus(string message, StatusType type)
        {
            StatusMessage.Text = message;
            StatusIcon.Visibility = Visibility.Visible;

            switch (type)
            {
                case StatusType.Info:
                    StatusIcon.Text = "ℹ️";
                    StatusMessage.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));
                    break;
                case StatusType.Loading:
                    StatusIcon.Text = "⏳";
                    StatusMessage.Foreground = new SolidColorBrush(Color.FromRgb(79, 70, 229));
                    break;
                case StatusType.Success:
                    StatusIcon.Text = "✅";
                    StatusMessage.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129));
                    break;
                case StatusType.Error:
                    StatusIcon.Text = "❌";
                    StatusMessage.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68));
                    break;
            }
        }

        private void SetLoading(bool isLoading)
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            ExecuteButton.IsEnabled = !isLoading;
            ClearButton.IsEnabled = !isLoading;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        #endregion
    }

    /// <summary>
    /// ViewModel للمنتج في قائمة تحديث المخزون
    /// </summary>
    public class StockInItemViewModel : INotifyPropertyChanged
    {
        private string _productname = "";
        private int _quantity = 1;
        private bool? _isValid = null;

        public string productname
        {
            get => _productname;
            set
            {
                _productname = value;
                OnPropertyChanged();
            }
        }

        public int quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// تحديد ما إذا كان المنتج موجود في النظام
        /// null = لم يتم التحقق، true = موجود، false = غير موجود
        /// </summary>
        public bool? IsValid
        {
            get => _isValid;
            set
            {
                _isValid = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
