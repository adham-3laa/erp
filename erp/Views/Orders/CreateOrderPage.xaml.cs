using erp.DTOS.Orders;
using erp.DTOS;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace erp.Views.Orders
{
    public partial class CreateOrderPage : Page
    {
        private readonly OrdersService _ordersService;
        private readonly UserService _userService;

        // 🔹 مصدر البيانات للـ DataGrid
        private readonly List<CreateOrderItemDto> _items =
            new List<CreateOrderItemDto>();

        // ✅ قوائم العملاء والمندوبين
        private List<string> _allCustomers = new List<string>();
        private List<string> _allSalesReps = new List<string>();

        public CreateOrderPage()
        {
            InitializeComponent();

            _ordersService = new OrdersService(App.Api);
            _userService = new UserService(App.Api);

            // صف افتراضي
            _items.Add(new CreateOrderItemDto());
            ItemsGrid.ItemsSource = _items;

            OrdersTopBarControl.ApprovedOrdersClicked += (_, __) =>
                NavigationService.Navigate(new ApprovedOrdersPage());

            // تحميل قوائم العملاء والمندوبين
            Loaded += CreateOrderPage_Loaded;
        }

        private async void CreateOrderPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadCustomersAndSalesRepsAsync();
        }

        private async Task LoadCustomersAndSalesRepsAsync()
        {
            try
            {
                // جلب العملاء
                var customersResponse = await _userService.GetUsersAsync(
                    userType: "Customer",
                    isActive: true,
                    page: 1,
                    pageSize: 1000);

                _allCustomers = customersResponse?.Users
                    ?.Where(u => !string.IsNullOrWhiteSpace(u.Fullname))
                    .Select(u => u.Fullname.Trim())
                    .Distinct()
                    .ToList() ?? new List<string>();

                // جلب المندوبين
                var salesRepsResponse = await _userService.GetUsersAsync(
                    userType: "SalesRep",
                    isActive: true,
                    page: 1,
                    pageSize: 1000);

                _allSalesReps = salesRepsResponse?.Users
                    ?.Where(u => !string.IsNullOrWhiteSpace(u.Fullname))
                    .Select(u => u.Fullname.Trim())
                    .Distinct()
                    .ToList() ?? new List<string>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل البيانات: {ex.Message}", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // 🔢 أرقام فقط (للعمولة - تقبل double)
        private void CommissionTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;

            string newText = textBox.Text.Insert(textBox.SelectionStart, e.Text);
            
            // السماح بالأرقام والنقطة
            bool isValid = newText.All(c => char.IsDigit(c) || c == '.' || c == ',');
            
            if (isValid)
            {
                // التحقق من وجود نقطة واحدة فقط
                int dotCount = newText.Count(c => c == '.' || c == ',');
                if (dotCount > 1)
                {
                    e.Handled = true;
                    return;
                }
                
                // السماح بكتابة النقطة
                e.Handled = false;
            }
            else
            {
                e.Handled = true;
            }
        }

        // ➕ إضافة منتج
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            _items.Add(new CreateOrderItemDto());
            ItemsGrid.Items.Refresh();
        }

        // ❌ حذف منتج
        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn &&
                btn.DataContext is CreateOrderItemDto item)
            {
                _items.Remove(item);
                ItemsGrid.Items.Refresh();
            }
        }

        private async void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // التحقق من اسم العميل
                var customerName = CustomerNameTextBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(customerName))
                {
                    MessageBox.Show("من فضلك أدخل اسم العميل", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CustomerNameTextBox.Focus();
                    return;
                }

                // التحقق من أن اسم العميل ثلاثي (يحتوي على مسافتين على الأقل)
                var nameParts = customerName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (nameParts.Length < 3)
                {
                    MessageBox.Show("اسم العميل يجب أن يكون ثلاثي (يحتوي على مسافتين على الأقل)", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CustomerNameTextBox.Focus();
                    return;
                }

                // التحقق من أن اسم العميل موجود في النظام
                if (!_allCustomers.Contains(customerName))
                {
                    MessageBox.Show("اسم العميل غير موجود في النظام", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CustomerNameTextBox.Focus();
                    return;
                }

                // التحقق من اسم المندوب
                var salesRepName = SalesRepNameTextBox.Text?.Trim();
                if (string.IsNullOrWhiteSpace(salesRepName))
                {
                    MessageBox.Show("من فضلك أدخل اسم المندوب", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SalesRepNameTextBox.Focus();
                    return;
                }

                // التحقق من أن اسم المندوب موجود في النظام
                if (!_allSalesReps.Contains(salesRepName))
                {
                    MessageBox.Show("اسم المندوب غير موجود في النظام", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    SalesRepNameTextBox.Focus();
                    return;
                }

                // التحقق من نسبة العمولة (double)
                if (string.IsNullOrWhiteSpace(CommissionTextBox.Text))
                {
                    MessageBox.Show("من فضلك أدخل نسبة العمولة", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CommissionTextBox.Focus();
                    return;
                }

                // تحويل النقطة العربية إلى إنجليزية
                var commissionText = CommissionTextBox.Text.Replace(',', '.');
                if (!double.TryParse(commissionText, NumberStyles.Float, CultureInfo.InvariantCulture, out var commission))
                {
                    MessageBox.Show("نسبة العمولة غير صحيحة. يرجى إدخال رقم صحيح", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CommissionTextBox.Focus();
                    return;
                }

                if (commission < 0 || commission > 100)
                {
                    MessageBox.Show("نسبة العمولة يجب أن تكون بين 0 و 100", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    CommissionTextBox.Focus();
                    return;
                }

                // التحقق من المنتجات
                var validItems = _items
                    .Where(i => !string.IsNullOrWhiteSpace(i.productname) && i.quantity > 0)
                    .ToList();

                if (!validItems.Any())
                {
                    MessageBox.Show("من فضلك أدخل منتج واحد على الأقل", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var request = new CreateOrderRequestDto
                {
                    customername = customerName,
                    salesrepname = salesRepName,
                    items = validItems
                };

                // إنشاء الطلب
                await _ordersService.CreateOrderAsync(request, commission);
                MessageBox.Show("تم إنشاء الطلب بنجاح ✅", "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ أثناء إنشاء الطلب:\n{ex.Message}", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ClearForm()
        {
            CustomerNameTextBox.Clear();
            SalesRepNameTextBox.Clear();
            CommissionTextBox.Clear();
            CustomerNumberTextBox.Clear();

            _items.Clear();
            _items.Add(new CreateOrderItemDto());
            ItemsGrid.Items.Refresh();
        }

        // ================== Customer AutoComplete Logic ==================
        private void CustomerNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CustomerNameTextBox == null || string.IsNullOrEmpty(CustomerNameTextBox.Text))
            {
                CustomerSuggestionsBorder.Visibility = Visibility.Collapsed;
                return;
            }

            var searchText = CustomerNameTextBox.Text.Trim();
            var filtered = _allCustomers
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
                CustomerSuggestionsListBox.ItemsSource = filtered;
                CustomerSuggestionsBorder.Visibility = Visibility.Visible;
            }
            else
            {
                CustomerSuggestionsBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void CustomerNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CustomerNameTextBox.Text))
            {
                CustomerNameTextBox_TextChanged(sender, null);
            }
        }

        private void CustomerNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var focusedElement = FocusManager.GetFocusedElement(this);
            if (focusedElement != CustomerSuggestionsListBox && focusedElement != CustomerNameTextBox)
            {
                Task.Delay(150).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (!CustomerSuggestionsListBox.IsMouseOver && !CustomerNameTextBox.IsFocused)
                        {
                            CustomerSuggestionsBorder.Visibility = Visibility.Collapsed;
                        }
                    });
                });
            }
        }

        private void CustomerNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (CustomerSuggestionsBorder.Visibility != Visibility.Visible)
                return;

            if (e.Key == Key.Down)
            {
                if (CustomerSuggestionsListBox.Items.Count > 0)
                {
                    CustomerSuggestionsListBox.Focus();
                    if (CustomerSuggestionsListBox.SelectedIndex < CustomerSuggestionsListBox.Items.Count - 1)
                        CustomerSuggestionsListBox.SelectedIndex++;
                    else
                        CustomerSuggestionsListBox.SelectedIndex = 0;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (CustomerSuggestionsListBox.Items.Count > 0)
                {
                    CustomerSuggestionsListBox.Focus();
                    if (CustomerSuggestionsListBox.SelectedIndex > 0)
                        CustomerSuggestionsListBox.SelectedIndex--;
                    else
                        CustomerSuggestionsListBox.SelectedIndex = CustomerSuggestionsListBox.Items.Count - 1;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (CustomerSuggestionsListBox.SelectedItem != null)
                {
                    SelectCustomerSuggestion(CustomerSuggestionsListBox.SelectedItem.ToString());
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                CustomerSuggestionsBorder.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void CustomerSuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void CustomerSuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (CustomerSuggestionsListBox.SelectedItem != null)
            {
                SelectCustomerSuggestion(CustomerSuggestionsListBox.SelectedItem.ToString());
            }
        }

        private void CustomerSuggestionsListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void SelectCustomerSuggestion(string selectedCustomer)
        {
            CustomerNameTextBox.Text = selectedCustomer;
            CustomerSuggestionsBorder.Visibility = Visibility.Collapsed;
            CustomerNameTextBox.Focus();
        }

        // ================== Sales Rep AutoComplete Logic ==================
        private void SalesRepNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SalesRepNameTextBox == null || string.IsNullOrEmpty(SalesRepNameTextBox.Text))
            {
                SalesRepSuggestionsBorder.Visibility = Visibility.Collapsed;
                return;
            }

            var searchText = SalesRepNameTextBox.Text.Trim();
            var filtered = _allSalesReps
                .Where(s => s.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s =>
                {
                    if (s.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                        return 0;
                    if (s.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                        return 1;
                    return 2;
                })
                .ThenBy(s => s.Length)
                .Take(10)
                .ToList();

            if (filtered.Count > 0)
            {
                SalesRepSuggestionsListBox.ItemsSource = filtered;
                SalesRepSuggestionsBorder.Visibility = Visibility.Visible;
            }
            else
            {
                SalesRepSuggestionsBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void SalesRepNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SalesRepNameTextBox.Text))
            {
                SalesRepNameTextBox_TextChanged(sender, null);
            }
        }

        private void SalesRepNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var focusedElement = FocusManager.GetFocusedElement(this);
            if (focusedElement != SalesRepSuggestionsListBox && focusedElement != SalesRepNameTextBox)
            {
                Task.Delay(150).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (!SalesRepSuggestionsListBox.IsMouseOver && !SalesRepNameTextBox.IsFocused)
                        {
                            SalesRepSuggestionsBorder.Visibility = Visibility.Collapsed;
                        }
                    });
                });
            }
        }

        private void SalesRepNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (SalesRepSuggestionsBorder.Visibility != Visibility.Visible)
                return;

            if (e.Key == Key.Down)
            {
                if (SalesRepSuggestionsListBox.Items.Count > 0)
                {
                    SalesRepSuggestionsListBox.Focus();
                    if (SalesRepSuggestionsListBox.SelectedIndex < SalesRepSuggestionsListBox.Items.Count - 1)
                        SalesRepSuggestionsListBox.SelectedIndex++;
                    else
                        SalesRepSuggestionsListBox.SelectedIndex = 0;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (SalesRepSuggestionsListBox.Items.Count > 0)
                {
                    SalesRepSuggestionsListBox.Focus();
                    if (SalesRepSuggestionsListBox.SelectedIndex > 0)
                        SalesRepSuggestionsListBox.SelectedIndex--;
                    else
                        SalesRepSuggestionsListBox.SelectedIndex = SalesRepSuggestionsListBox.Items.Count - 1;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (SalesRepSuggestionsListBox.SelectedItem != null)
                {
                    SelectSalesRepSuggestion(SalesRepSuggestionsListBox.SelectedItem.ToString());
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                SalesRepSuggestionsBorder.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void SalesRepSuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void SalesRepSuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SalesRepSuggestionsListBox.SelectedItem != null)
            {
                SelectSalesRepSuggestion(SalesRepSuggestionsListBox.SelectedItem.ToString());
            }
        }

        private void SalesRepSuggestionsListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void SelectSalesRepSuggestion(string selectedSalesRep)
        {
            SalesRepNameTextBox.Text = selectedSalesRep;
            SalesRepSuggestionsBorder.Visibility = Visibility.Collapsed;
            SalesRepNameTextBox.Focus();
        }
    }
}
