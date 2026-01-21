using erp.Services;
using erp.DTOS;
using erp.DTOS.Inventory.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using EduGate.Models;
using static MaterialDesignThemes.Wpf.Theme.ToolBar;
using System.Collections.ObjectModel;

namespace erp.Views.Inventory
{
    public partial class StockInProductsPage : Page
    {
        private readonly InventoryService _service = new();
        private readonly UserService _userService = new UserService(App.Api);

        // ✅ قوائم المنتجات والموردين
        private List<string> _allProducts = new List<string>();
        private List<string> _allSuppliers = new List<string>();

        private ObservableCollection<StockInItemRequest> _products =
        new ObservableCollection<StockInItemRequest>();


        public StockInProductsPage()
        {
            InitializeComponent();

            _products.Add(new StockInItemRequest()); // صف جاهز
            ProductsGrid.ItemsSource = _products;

            Loaded += StockInProductsPage_Loaded;
        }


        private async void StockInProductsPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAllProductsAndSuppliersAsync();
        }

        private async Task LoadAllProductsAndSuppliersAsync()
        {
            try
            {
                // جلب المنتجات
                var products = await _service.GetAllProductsLookupAsync();
                _allProducts = products
                    .Where(p => !string.IsNullOrWhiteSpace(p.ProductName))
                    .Select(p => p.ProductName.Trim())
                    .Distinct()
                    .ToList();

                // جلب الموردين
                var suppliersResponse = await _userService.GetUsersAsync(
                    userType: "Supplier",
                    isActive: true,
                    page: 1,
                    pageSize: 1000);

                _allSuppliers = suppliersResponse?.Users
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

        // ================== Supplier AutoComplete Logic ==================
        private void SupplierNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (SupplierNameTextBox == null || string.IsNullOrEmpty(SupplierNameTextBox.Text))
            {
                SupplierSuggestionsBorder.Visibility = Visibility.Collapsed;
                return;
            }

            var searchText = SupplierNameTextBox.Text.Trim();
            var filtered = _allSuppliers
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
                SupplierSuggestionsListBox.ItemsSource = filtered;
                SupplierSuggestionsBorder.Visibility = Visibility.Visible;
            }
            else
            {
                SupplierSuggestionsBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void SupplierNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(SupplierNameTextBox.Text))
            {
                SupplierNameTextBox_TextChanged(sender, null);
            }
        }

        private void SupplierNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var focusedElement = FocusManager.GetFocusedElement(this);
            if (focusedElement != SupplierSuggestionsListBox && focusedElement != SupplierNameTextBox)
            {
                Task.Delay(150).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (!SupplierSuggestionsListBox.IsMouseOver && !SupplierNameTextBox.IsFocused)
                        {
                            SupplierSuggestionsBorder.Visibility = Visibility.Collapsed;
                        }
                    });
                });
            }
        }

        private void SupplierNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (SupplierSuggestionsBorder.Visibility != Visibility.Visible)
                return;

            if (e.Key == Key.Down)
            {
                if (SupplierSuggestionsListBox.Items.Count > 0)
                {
                    SupplierSuggestionsListBox.Focus();
                    if (SupplierSuggestionsListBox.SelectedIndex < SupplierSuggestionsListBox.Items.Count - 1)
                        SupplierSuggestionsListBox.SelectedIndex++;
                    else
                        SupplierSuggestionsListBox.SelectedIndex = 0;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (SupplierSuggestionsListBox.Items.Count > 0)
                {
                    SupplierSuggestionsListBox.Focus();
                    if (SupplierSuggestionsListBox.SelectedIndex > 0)
                        SupplierSuggestionsListBox.SelectedIndex--;
                    else
                        SupplierSuggestionsListBox.SelectedIndex = SupplierSuggestionsListBox.Items.Count - 1;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (SupplierSuggestionsListBox.SelectedItem != null)
                {
                    SelectSupplierSuggestion(SupplierSuggestionsListBox.SelectedItem.ToString());
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                SupplierSuggestionsBorder.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void SupplierSuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void SupplierSuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SupplierSuggestionsListBox.SelectedItem != null)
            {
                SelectSupplierSuggestion(SupplierSuggestionsListBox.SelectedItem.ToString());
            }
        }

        private void SupplierSuggestionsListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void SelectSupplierSuggestion(string selectedSupplier)
        {
            SupplierNameTextBox.Text = selectedSupplier;
            SupplierSuggestionsBorder.Visibility = Visibility.Collapsed;
            SupplierNameTextBox.Focus();
        }

        // ================== Product AutoComplete Logic in DataGrid ==================
        private TextBox _currentProductTextBox;
        private StockInItemRequest _currentProductItem;

        private void ProductNameCell_TextChanged(object sender, TextChangedEventArgs e)
        {
            StockInItemRequest item = null;

            if (sender is TextBox textBox)
            {
                item = textBox.DataContext as StockInItemRequest;
                if (item == null) return;

                _currentProductTextBox = textBox;
                _currentProductItem = item;

                if (string.IsNullOrEmpty(textBox.Text))
                {
                    ProductSuggestionsPopup.IsOpen = false;
                    return;
                }

                var searchText = textBox.Text.Trim();
                var filtered = _allProducts
                    .Where(p => p.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(p =>
                    {
                        if (p.Equals(searchText, StringComparison.OrdinalIgnoreCase))
                            return 0;
                        if (p.StartsWith(searchText, StringComparison.OrdinalIgnoreCase))
                            return 1;
                        return 2;
                    })
                    .ThenBy(p => p.Length)
                    .Take(10)
                    .ToList();

                if (filtered.Count > 0)
                {
                    ProductSuggestionsPopupListBox.ItemsSource = filtered;
                    ProductSuggestionsPopup.PlacementTarget = textBox;
                    ProductSuggestionsPopup.IsOpen = true;
                }
                else
                {
                    ProductSuggestionsPopup.IsOpen = false;
                }
            }

            // ✅ دلوقتي item متاح هنا
            if (item != null &&
                _products.Last() == item &&
                !string.IsNullOrWhiteSpace(item.productname))
            {
                _products.Add(new StockInItemRequest());
            }
        }


        private void ProductNameCell_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                _currentProductTextBox = textBox;
                _currentProductItem = textBox.DataContext as StockInItemRequest;
                
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    ProductNameCell_TextChanged(sender, null);
                }
            }
        }

        private void ProductNameCell_LostFocus(object sender, RoutedEventArgs e)
        {
            Task.Delay(150).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (ProductSuggestionsPopupListBox != null && 
                        !ProductSuggestionsPopupListBox.IsMouseOver && 
                        (_currentProductTextBox == null || !_currentProductTextBox.IsFocused))
                    {
                        ProductSuggestionsPopup.IsOpen = false;
                    }
                });
            });
        }

        private void ProductNameCell_KeyDown(object sender, KeyEventArgs e)
        {
            if (!ProductSuggestionsPopup.IsOpen)
                return;

            if (e.Key == Key.Down)
            {
                if (ProductSuggestionsPopupListBox.Items.Count > 0)
                {
                    ProductSuggestionsPopupListBox.Focus();
                    if (ProductSuggestionsPopupListBox.SelectedIndex < ProductSuggestionsPopupListBox.Items.Count - 1)
                        ProductSuggestionsPopupListBox.SelectedIndex++;
                    else
                        ProductSuggestionsPopupListBox.SelectedIndex = 0;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (ProductSuggestionsPopupListBox.Items.Count > 0)
                {
                    ProductSuggestionsPopupListBox.Focus();
                    if (ProductSuggestionsPopupListBox.SelectedIndex > 0)
                        ProductSuggestionsPopupListBox.SelectedIndex--;
                    else
                        ProductSuggestionsPopupListBox.SelectedIndex = ProductSuggestionsPopupListBox.Items.Count - 1;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (ProductSuggestionsPopupListBox.SelectedItem != null && _currentProductItem != null)
                {
                    _currentProductItem.productname = ProductSuggestionsPopupListBox.SelectedItem.ToString();
                    if (_currentProductTextBox != null)
                        _currentProductTextBox.Text = _currentProductItem.productname;
                    ProductSuggestionsPopup.IsOpen = false;
                    if (_currentProductTextBox != null)
                        _currentProductTextBox.Focus();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                ProductSuggestionsPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void ProductSuggestionsPopupListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ProductSuggestionsPopupListBox.SelectedItem != null && _currentProductItem != null)
            {
                var selected = ProductSuggestionsPopupListBox.SelectedItem.ToString();

                _currentProductItem.productname = selected;
                _currentProductTextBox.Text = selected;

                ProductSuggestionsPopup.IsOpen = false;
                ProductsGrid.CommitEdit();
            }
        }


        private void ProductSuggestionsPopupListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }
    }
}
