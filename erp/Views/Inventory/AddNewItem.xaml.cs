using EduGate.Models;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using erp.Services.Category;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace erp.Views.Inventory
{
    public partial class AddNewItem : Page
    {
        private readonly InventoryService _service = new();
        private readonly OrdersService _ordersService;
        private readonly CategoryService _categoryService;
        private readonly List<Product> _products = new();

        private int _count;
        private int _index;

        // اسم المورد المحدد
        private string _supplierName = "";
        private SupplierAutocompleteItem? _selectedSupplier;

        // كل الأصناف الموجودة في السيستم
        private List<string> _allCategories = new();

        // Suggestions for autocomplete
        private List<SupplierAutocompleteItem> _supplierSuggestions = new();

        // Debounce timers
        private CancellationTokenSource? _supplierSearchCts;

        // ثابت لوقت التأخير في البحث (بالمللي ثانية)
        private const int SearchDebounceMs = 300;

        public AddNewItem()
        {
            InitializeComponent();
            _ordersService = new OrdersService(App.Api);
            _categoryService = new CategoryService(App.Api);
            UpdatePlaceholders();
        }

        #region === Placeholders ===

        private void UpdatePlaceholders()
        {
            if (SupplierPlaceholder != null)
                SupplierPlaceholder.Visibility = string.IsNullOrEmpty(SupplierNameTextBox?.Text)
                    ? Visibility.Visible : Visibility.Collapsed;

            if (CategoryPlaceholder != null && CategoryTextBox != null)
                CategoryPlaceholder.Visibility = string.IsNullOrEmpty(CategoryTextBox.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region === Step Navigation ===

        // زرار "ابدأ الآن"
        private void ShowCount_Click(object sender, RoutedEventArgs e)
        {
            IntroPanel.Visibility = Visibility.Collapsed;
            CountPanel.Visibility = Visibility.Visible;
            ClearAllErrors();
        }

        // زرار "ابدأ إدخال المنتجات"
        private async void StartWizard_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();

            // التحقق من اسم المورد
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

            // التحقق من عدد المنتجات
            if (!int.TryParse(CountTextBox.Text, out _count) || _count <= 0)
            {
                ShowError(CountErrorText, null, "من فضلك أدخل عدد صحيح للمنتجات (1 أو أكثر)");
                CountTextBox.Focus();
                return;
            }

            if (_count > 100)
            {
                ShowError(CountErrorText, null, "الحد الأقصى لعدد المنتجات هو 100 منتج");
                CountTextBox.Focus();
                return;
            }

            _supplierName = supplierName;

            // إنشاء قائمة المنتجات
            _products.Clear();
            for (int i = 0; i < _count; i++)
                _products.Add(new Product { Quantity = 1 });

            // تحميل الأصناف
            SetLoading(true, "جاري تحميل التصنيفات...");
            await LoadAllCategoriesAsync();
            SetLoading(false);

            // الانتقال لنموذج المنتجات
            CountPanel.Visibility = Visibility.Collapsed;
            FormPanel.Visibility = Visibility.Visible;
            PrevBtn.Visibility = Visibility.Visible;
            NextBtn.Visibility = Visibility.Visible;

            SupplierDisplayText.Text = $"المورد: {_supplierName}";

            _index = 0;
            LoadCurrent();
        }

        private async Task LoadAllCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllAsync();
                _allCategories = categories
                    .Select(c => c.Name)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList();
            }
            catch (Exception ex)
            {
                _allCategories = new List<string>();
                SetStatus($"فشل في تحميل التصنيفات", StatusType.Error);
            }
        }

        private void LoadCurrent()
        {
            if (_products.Count == 0 || _index < 0 || _index >= _products.Count)
                return;

            DataContext = _products[_index];
            StepTitle.Text = $"المنتج {_index + 1} من {_count}";

            // تحديث الأزرار
            PrevBtn.Visibility = Visibility.Visible;
            NextBtn.Visibility = _index < _count - 1 ? Visibility.Visible : Visibility.Collapsed;
            SaveBtn.Visibility = _index == _count - 1 ? Visibility.Visible : Visibility.Collapsed;

            // تحديث الـ placeholder
            UpdatePlaceholders();
            ClearError(ProductErrorText, null);
        }

        private void Next_Click(object sender, RoutedEventArgs e)
        {
            if (_products.Count == 0 || _index < 0 || _index >= _products.Count)
                return;

            var product = _products[_index];

            if (!IsProductValid(product))
                return;

            if (_index < _count - 1)
            {
                _index++;
                LoadCurrent();
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (_index > 0)
            {
                _index--;
                LoadCurrent();
            }
            else
            {
                // العودة لصفحة بيانات المورد
                FormPanel.Visibility = Visibility.Collapsed;
                CountPanel.Visibility = Visibility.Visible;
                PrevBtn.Visibility = Visibility.Collapsed;
                NextBtn.Visibility = Visibility.Collapsed;
                SaveBtn.Visibility = Visibility.Collapsed;
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_products.Count == 0 || _index < 0 || _index >= _products.Count)
                return;

            var product = _products[_index];

            if (!IsProductValid(product))
                return;

            try
            {
                SetLoading(true, "جاري حفظ المنتجات...");
                SetStatus("جاري حفظ المنتجات...", StatusType.Loading);

                await _service.AddProductsWithCategoryNameAsync(_products, _supplierName);

                SetLoading(false);
                SetStatus("تم حفظ المنتجات بنجاح! ✅", StatusType.Success);

                MessageBox.Show(
                    $"تم إضافة {_count} منتج بنجاح ✅",
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                SetLoading(false);
                SetStatus("فشل في حفظ المنتجات", StatusType.Error);

                MessageBox.Show(
                    $"حدث خطأ أثناء حفظ المنتجات:\n{ex.Message}",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        #endregion

        #region === Validation ===

        private bool IsProductValid(Product product)
        {
            ClearError(ProductErrorText, null);

            if (product == null)
            {
                ShowError(ProductErrorText, null, "بيانات المنتج غير صالحة");
                return false;
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ShowError(ProductErrorText, null, "اسم المنتج مطلوب");
                return false;
            }

            if (product.SalePrice <= 0)
            {
                ShowError(ProductErrorText, null, "سعر البيع يجب أن يكون أكبر من صفر");
                return false;
            }

            if (product.BuyPrice <= 0)
            {
                ShowError(ProductErrorText, null, "سعر الشراء يجب أن يكون أكبر من صفر");
                return false;
            }

            if (product.Quantity <= 0)
            {
                ShowError(ProductErrorText, null, "الكمية يجب أن تكون أكبر من صفر");
                return false;
            }

            // التحقق من الصنف
            if (string.IsNullOrWhiteSpace(product.Category))
            {
                ShowError(ProductErrorText, null, "التصنيف مطلوب");
                return false;
            }

            if (!_allCategories.Contains(product.Category))
            {
                ShowError(ProductErrorText, null, $"التصنيف '{product.Category}' غير موجود في النظام. اختر من القائمة المتاحة");
                return false;
            }

            return true;
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

        #region === Category Autocomplete ===

        private void CategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholders();

            if (CategoryTextBox == null || string.IsNullOrEmpty(CategoryTextBox.Text))
            {
                CategorySuggestionsPopup.IsOpen = false;
                return;
            }

            var searchText = CategoryTextBox.Text.Trim();

            // ترتيب النتائج
            var filtered = _allCategories
                .Where(c => c.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(c =>
                {
                    if (c.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                        return 0;
                    if (c.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                        return 1;
                    return 2;
                })
                .ThenBy(c => c.Length)
                .Take(10)
                .ToList();

            if (filtered.Count > 0)
            {
                SuggestionsListBox.ItemsSource = filtered;
                SuggestionsListBox.Visibility = Visibility.Visible;
                CategoryNoResultsText.Visibility = Visibility.Collapsed;
                CategorySuggestionsPopup.IsOpen = true;
            }
            else
            {
                SuggestionsListBox.Visibility = Visibility.Collapsed;
                CategoryNoResultsText.Visibility = Visibility.Visible;
                CategorySuggestionsPopup.IsOpen = true;
            }
        }

        private void CategoryTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CategoryTextBox.Text))
            {
                CategoryTextBox_TextChanged(sender, null!);
            }
        }

        private void CategoryTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Task.Delay(200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!SuggestionsListBox.IsMouseOver && !CategoryTextBox.IsFocused)
                    {
                        CategorySuggestionsPopup.IsOpen = false;
                    }
                });
            });
        }

        private void CategoryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!CategorySuggestionsPopup.IsOpen) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (SuggestionsListBox.Items.Count > 0)
                    {
                        SuggestionsListBox.Focus();
                        SuggestionsListBox.SelectedIndex = Math.Min(
                            SuggestionsListBox.SelectedIndex + 1,
                            SuggestionsListBox.Items.Count - 1);
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (SuggestionsListBox.Items.Count > 0)
                    {
                        SuggestionsListBox.Focus();
                        SuggestionsListBox.SelectedIndex = Math.Max(
                            SuggestionsListBox.SelectedIndex - 1, 0);
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (SuggestionsListBox.SelectedItem != null)
                    {
                        SelectCategory(SuggestionsListBox.SelectedItem.ToString()!);
                    }
                    e.Handled = true;
                    break;

                case Key.Escape:
                    CategorySuggestionsPopup.IsOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        private void SuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem != null)
            {
                SelectCategory(SuggestionsListBox.SelectedItem.ToString()!);
            }
        }

        private void SuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem != null)
            {
                SelectCategory(SuggestionsListBox.SelectedItem.ToString()!);
            }
        }

        private void SuggestionsListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void SelectCategory(string selectedCategory)
        {
            if (DataContext is Product product)
            {
                product.Category = selectedCategory;
            }
            CategoryTextBox.Text = selectedCategory;
            CategorySuggestionsPopup.IsOpen = false;
            CategoryTextBox.Focus();
            CategoryTextBox.CaretIndex = CategoryTextBox.Text.Length;
            UpdatePlaceholders();
        }

        #endregion

        #region === Input Validation ===

        private void CountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
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
            ClearError(CountErrorText, null);
            if (ProductErrorText != null)
                ClearError(ProductErrorText, null);
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

        private void SetLoading(bool isLoading, string? message = null)
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            if (!string.IsNullOrEmpty(message))
                LoadingText.Text = message;

            NextBtn.IsEnabled = !isLoading;
            SaveBtn.IsEnabled = !isLoading;
            PrevBtn.IsEnabled = !isLoading;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        #endregion
    }
}
