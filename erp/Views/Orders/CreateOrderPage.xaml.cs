using erp.DTOS.Orders;
using erp.Services;
using erp.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace erp.Views.Orders
{
    public partial class CreateOrderPage : Page
    {
        private readonly OrdersService _ordersService;

        // مصدر البيانات للمنتجات
        private readonly ObservableCollection<OrderItemViewModel> _items = new();

        // حقول للـ Autocomplete
        private List<CustomerAutocompleteItem> _customerSuggestions = new();
        private List<ProductAutocompleteItem> _productSuggestions = new();

        // Debounce timers
        private CancellationTokenSource? _customerSearchCts;
        private CancellationTokenSource? _productSearchCts;

        // المنتج الحالي المحدد للـ Autocomplete
        private TextBox? _currentProductTextBox;
        private OrderItemViewModel? _currentProductItem;

        // العميل المحدد
        private CustomerAutocompleteItem? _selectedCustomer;

        // ثابت لوقت التأخير في البحث (بالمللي ثانية)
        private const int SearchDebounceMs = 300;

        public CreateOrderPage()
        {
            InitializeComponent();

            _ordersService = new OrdersService(App.Api);

            // إضافة منتج افتراضي
            _items.Add(new OrderItemViewModel());
            ItemsControl.ItemsSource = _items;
            UpdateItemsCount();

            // ربط أحداث الـ TopBar
            OrdersTopBarControl.ApprovedOrdersClicked += (_, __) =>
                NavigationService?.Navigate(new ApprovedOrdersPage());

            // تحديث الـ Placeholders
            Loaded += (_, __) => UpdatePlaceholders();
        }

        #region === Placeholders ===

        private void UpdatePlaceholders()
        {
            CustomerPlaceholder.Visibility = string.IsNullOrEmpty(CustomerNameTextBox.Text)
                ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region === Customer Autocomplete ===

        private async void CustomerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholders();
            ClearError(CustomerErrorText, CustomerInputWrapper);

            var searchText = CustomerNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                CustomerSuggestionsPopup.IsOpen = false;
                _selectedCustomer = null;
                return;
            }

            // إلغاء البحث السابق
            _customerSearchCts?.Cancel();
            _customerSearchCts = new CancellationTokenSource();
            var token = _customerSearchCts.Token;

            try
            {
                // إظهار حالة التحميل
                ShowCustomerLoading(true);
                CustomerSuggestionsPopup.IsOpen = true;

                await Task.Delay(SearchDebounceMs, token);
                if (token.IsCancellationRequested) return;

                // البحث من الـ API
                _customerSuggestions = await _ordersService.GetCustomersAutocompleteAsync(searchText);

                if (token.IsCancellationRequested) return;

                ShowCustomerLoading(false);

                if (_customerSuggestions.Count > 0)
                {
                    CustomerSuggestionsListBox.ItemsSource = _customerSuggestions;
                    CustomerSuggestionsListBox.Visibility = Visibility.Visible;
                    CustomerNoResultsText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    CustomerSuggestionsListBox.Visibility = Visibility.Collapsed;
                    CustomerNoResultsText.Visibility = Visibility.Visible;
                }
            }
            catch (OperationCanceledException)
            {
                // تم الإلغاء - لا شيء للقيام به
            }
            catch (Exception ex)
            {
                ShowCustomerLoading(false);
                CustomerNoResultsText.Text = "خطأ في البحث";
                CustomerNoResultsText.Visibility = Visibility.Visible;
                CustomerSuggestionsListBox.Visibility = Visibility.Collapsed;
            }
        }

        private void ShowCustomerLoading(bool show)
        {
            CustomerLoadingText.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
            CustomerSuggestionsListBox.Visibility = show ? Visibility.Collapsed : Visibility.Visible;
            CustomerNoResultsText.Visibility = Visibility.Collapsed;
        }

        private void CustomerNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CustomerNameTextBox.Text) && _customerSuggestions.Count > 0)
            {
                CustomerSuggestionsPopup.IsOpen = true;
            }
        }

        private void CustomerNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // تأخير الإغلاق للسماح بالنقر على العناصر
            Task.Delay(200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!CustomerSuggestionsListBox.IsMouseOver && !CustomerNameTextBox.IsFocused)
                    {
                        CustomerSuggestionsPopup.IsOpen = false;
                    }
                });
            });
        }

        private void CustomerNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!CustomerSuggestionsPopup.IsOpen) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (CustomerSuggestionsListBox.Items.Count > 0)
                    {
                        CustomerSuggestionsListBox.Focus();
                        CustomerSuggestionsListBox.SelectedIndex = Math.Min(
                            CustomerSuggestionsListBox.SelectedIndex + 1,
                            CustomerSuggestionsListBox.Items.Count - 1);
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (CustomerSuggestionsListBox.Items.Count > 0)
                    {
                        CustomerSuggestionsListBox.Focus();
                        CustomerSuggestionsListBox.SelectedIndex = Math.Max(
                            CustomerSuggestionsListBox.SelectedIndex - 1, 0);
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (CustomerSuggestionsListBox.SelectedItem is CustomerAutocompleteItem selected)
                    {
                        SelectCustomer(selected);
                    }
                    e.Handled = true;
                    break;

                case Key.Escape:
                    CustomerSuggestionsPopup.IsOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        private void CustomerSuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // لا نفعل شيء هنا - ننتظر النقر
        }

        private void CustomerSuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CustomerSuggestionsListBox.SelectedItem is CustomerAutocompleteItem selected)
            {
                SelectCustomer(selected);
            }
        }

        private void CustomerSuggestionsListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void SelectCustomer(CustomerAutocompleteItem customer)
        {
            _selectedCustomer = customer;
            CustomerNameTextBox.Text = customer.fullname;
            CustomerSuggestionsPopup.IsOpen = false;
            CustomerNameTextBox.Focus();
            CustomerNameTextBox.CaretIndex = CustomerNameTextBox.Text.Length;
            UpdatePlaceholders();
        }

        #endregion

        #region === Product Autocomplete ===

        private async void ProductNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is not TextBox textBox) return;
            if (textBox.Tag is not OrderItemViewModel item) return;

            _currentProductTextBox = textBox;
            _currentProductItem = item;

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
                // تحديد موقع الـ Popup
                ProductSuggestionsPopup.PlacementTarget = textBox;
                ShowProductLoading(true);
                ProductSuggestionsPopup.IsOpen = true;

                await Task.Delay(SearchDebounceMs, token);
                if (token.IsCancellationRequested) return;

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
            if (sender is TextBox textBox && textBox.Tag is OrderItemViewModel item)
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

        #region === Input Validation ===

        private void SellPriceTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // السماح بالأرقام والنقطة العشرية فقط
            var textBox = sender as TextBox;
            var newText = textBox?.Text.Insert(textBox.CaretIndex, e.Text) ?? e.Text;
            
            // التحقق من أن النص الناتج هو رقم صحيح أو عشري
            e.Handled = !decimal.TryParse(newText, out _);
        }

        private void PhoneNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        #endregion

        #region === Items Management ===

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            _items.Add(new OrderItemViewModel());
            UpdateItemsCount();
            ClearError(ProductsErrorText, null);
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is OrderItemViewModel item)
            {
                _items.Remove(item);
                UpdateItemsCount();
            }
        }

        private void UpdateItemsCount()
        {
            ItemsCountBadge.Text = _items.Count.ToString();
        }

        #endregion

        #region === Form Actions ===

        private async void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            // إعادة تعيين حالة الأخطاء
            ClearAllErrors();

            // التحقق من صحة البيانات
            if (!ValidateForm())
            {
                return;
            }

            try
            {
                // إظهار حالة التحميل
                SetLoading(true);
                SetStatus("جاري إنشاء الطلب...", StatusType.Loading);

                // تجميع البيانات
                var validItems = _items
                    .Where(i => !string.IsNullOrWhiteSpace(i.productname) && i.sellprice > 0 && i.quantity > 0)
                    .Select(i => new CreateOrderItemDto
                    {
                        productname = i.productname.Trim(),
                        sellprice = i.sellprice,
                        quantity = i.quantity
                    })
                    .ToList();

                var request = new CreateOrderRequestDto
                {
                    customername = CustomerNameTextBox.Text.Trim(),
                    salesrepname = "",
                    phonenumber = PhoneNumberTextBox.Text?.Trim() ?? "",
                    items = validItems
                };

                // إرسال الطلب
                await _ordersService.CreateOrderAsync(request, 0);

                // نجاح!
                SetLoading(false);
                SetStatus("تم إنشاء الطلب بنجاح! ✅", StatusType.Success);

                MessageBox.Show(
                    "تم إنشاء الطلب وتأكيده بنجاح ✅",
                    "نجاح",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                ClearForm();
            }
            catch (Exception ex)
            {
                SetLoading(false);
                
                // تعريب رسالة الخطأ
                var localizedError = ErrorMessageLocalizer.GetLocalizedErrorFromException(ex);
                
                SetStatus($"فشل في إنشاء الطلب", StatusType.Error);

                MessageBox.Show(
                    $"حدث خطأ أثناء إنشاء الطلب:\n{localizedError}",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool ValidateForm()
        {
            var isValid = true;

            // التحقق من اسم العميل
            var customerName = CustomerNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(customerName))
            {
                ShowError(CustomerErrorText, CustomerInputWrapper, "من فضلك أدخل اسم العميل");
                isValid = false;
            }
            else
            {
                // التحقق من أن الاسم ثلاثي (3 كلمات بالضبط)
                var nameParts = customerName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length != 3)
                {
                    ShowError(CustomerErrorText, CustomerInputWrapper, "اسم العميل يجب أن يكون ثلاثياً فقط (لا يقل ولا يزيد عن 3 أسماء)");
                    isValid = false;
                }
            }

            // التحقق من وجود منتج واحد على الأقل مع سعر البيع والكمية
            var validItems = _items
                .Where(i => !string.IsNullOrWhiteSpace(i.productname) && i.sellprice > 0 && i.quantity > 0)
                .ToList();

            if (validItems.Count == 0)
            {
                ShowError(ProductsErrorText, null, "من فضلك أضف منتجاً واحداً على الأقل مع تحديد سعر البيع والكمية");
                isValid = false;
            }

            // التحقق من أن سعر البيع لكل منتج صحيح
            var invalidPriceProducts = _items
                .Where(p => !string.IsNullOrWhiteSpace(p.productname) && p.sellprice <= 0)
                .ToList();

            if (invalidPriceProducts.Count > 0)
            {
                var invalidNames = string.Join("، ", invalidPriceProducts.Select(p => p.productname));
                ShowError(ProductsErrorText, null, $"يجب تحديد سعر البيع للمنتجات التالية: {invalidNames}");
                isValid = false;
            }

            return isValid;
        }

        private void ClearForm_Click(object sender, RoutedEventArgs e)
        {
            ClearForm();
            SetStatus("تم مسح النموذج", StatusType.Info);
        }

        private void ClearForm()
        {
            CustomerNameTextBox.Clear();
            PhoneNumberTextBox.Clear();

            _selectedCustomer = null;

            _items.Clear();
            _items.Add(new OrderItemViewModel());
            UpdateItemsCount();
            UpdatePlaceholders();
            ClearAllErrors();
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
                    Color.FromRgb(254, 202, 202), // #FECACA
                    Color.FromRgb(252, 165, 165), // #FCA5A5
                    45);
            }
        }

        private void ClearError(TextBlock errorTextBlock, Border? inputWrapper)
        {
            errorTextBlock.Visibility = Visibility.Collapsed;

            if (inputWrapper != null)
            {
                inputWrapper.Background = new LinearGradientBrush(
                    Color.FromRgb(229, 231, 235), // #E5E7EB
                    Color.FromRgb(209, 213, 219), // #D1D5DB
                    45);
            }
        }

        private void ClearAllErrors()
        {
            ClearError(CustomerErrorText, CustomerInputWrapper);
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
                    StatusMessage.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128)); // Gray
                    break;
                case StatusType.Loading:
                    StatusIcon.Text = "⏳";
                    StatusMessage.Foreground = new SolidColorBrush(Color.FromRgb(79, 70, 229)); // Indigo
                    break;
                case StatusType.Success:
                    StatusIcon.Text = "✅";
                    StatusMessage.Foreground = new SolidColorBrush(Color.FromRgb(16, 185, 129)); // Green
                    break;
                case StatusType.Error:
                    StatusIcon.Text = "❌";
                    StatusMessage.Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)); // Red
                    break;
            }
        }

        private void SetLoading(bool isLoading)
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            ConfirmButton.IsEnabled = !isLoading;
            ClearButton.IsEnabled = !isLoading;
        }

        #endregion
    }

    /// <summary>
    /// ViewModel للمنتج في قائمة الطلب
    /// </summary>
    public class OrderItemViewModel : INotifyPropertyChanged
    {
        private string _productname = "";
        private decimal _sellprice = 0;
        private int _quantity = 1;

        public string productname
        {
            get => _productname;
            set
            {
                _productname = value;
                OnPropertyChanged();
            }
        }

        public decimal sellprice
        {
            get => _sellprice;
            set
            {
                _sellprice = value;
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

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
