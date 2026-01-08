using erp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace erp.Views.Inventory
{
    public partial class InventoryCheckPage : Page
    {
        private readonly InventoryService _service = new();

        // ✅ قائمة أسماء المنتجات
        private List<string> _allProducts = new List<string>();

        public InventoryCheckPage()
        {
            InitializeComponent();
            Loaded += InventoryCheckPage_Loaded;
        }

        private async void InventoryCheckPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadAllProductsAsync();
        }

        private async Task LoadAllProductsAsync()
        {
            try
            {
                var products = await _service.GetAllProductsLookupAsync();
                _allProducts = products
                    .Where(p => !string.IsNullOrWhiteSpace(p.ProductName))
                    .Select(p => p.ProductName.Trim())
                    .Distinct()
                    .ToList();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"خطأ في تحميل قائمة المنتجات: {ex.Message}", "تحذير", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async void Adjust_Click(object sender, RoutedEventArgs e)
        {
            ResultCard.Visibility = Visibility.Collapsed;

            // ===== Validation =====
            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                ShowResultError("اسم المنتج مطلوب");
                return;
            }

            if (!int.TryParse(ActualQuantityTextBox.Text, out int actualQty) || actualQty < 0)
            {
                ShowResultError("الكمية الفعلية غير صحيحة");
                return;
            }

            bool updateStock = UpdateYesRadio.IsChecked == true;

            try
            {
                var result = await _service.AdjustInventoryByNameAsync(
                    ProductNameTextBox.Text.Trim(),
                    actualQty,
                    updateStock);

                ResultCard.Visibility = Visibility.Visible;

                // ===== Determine system quantity correctly =====
                int systemQty = updateStock
                    ? result.oldquantity          // بعد التحديث الحقيقي
                    : result.systemquantity;      // Preview فقط

                // ===== Display quantities =====
                if (updateStock)
                {
                    OldQuantityText.Text = $"الكمية القديمة: {result.oldquantity}";
                    NewQuantityText.Text = $"الكمية الجديدة: {result.newquantity}";
                }
                else
                {
                    OldQuantityText.Text = $"الكمية المسجلة في النظام: {result.systemquantity}";
                    NewQuantityText.Text = $"الكمية الفعلية: {actualQty}";
                }

                FinancialImpactText.Text = result.financialimpact;

                // ===== Correct result logic =====
                if (actualQty < systemQty)
                {
                    ResultTitleText.Text = "❌ نتيجة الجرد: نقص";
                    ResultMessageText.Text =
                        $"في نقص {systemQty - actualQty}";

                    ResultCard.BorderBrush = Brushes.Red;
                    ResultTitleText.Foreground = Brushes.Red;
                }
                else if (actualQty > systemQty)
                {
                    ResultTitleText.Text = "✅ نتيجة الجرد: زيادة";
                    ResultMessageText.Text =
                        $"في زيادة {actualQty - systemQty}";

                    ResultCard.BorderBrush = Brushes.Green;
                    ResultTitleText.Foreground = Brushes.Green;
                }
                else
                {
                    ResultTitleText.Text = "✔ نتيجة الجرد: متطابق";
                    ResultMessageText.Text =
                        "لا يوجد فرق بين الكمية الفعلية والمخزنة";

                    ResultCard.BorderBrush = Brushes.Gray;
                    ResultTitleText.Foreground = Brushes.Gray;
                }

                // ===== Preview note =====
                if (!updateStock)
                {
                    ResultMessageText.Text += "\n(لم يتم تحديث المخزون)";
                }
            }
            catch (Exception ex)
            {
                ShowResultError("خطأ أثناء الجرد:\n" + ex.Message);
            }
        }

        private void ShowResultError(string message)
        {
            ResultCard.Visibility = Visibility.Visible;

            ResultTitleText.Text = "خطأ";
            ResultMessageText.Text = message;

            OldQuantityText.Text = "";
            NewQuantityText.Text = "";
            FinancialImpactText.Text = "";

            ResultCard.BorderBrush = Brushes.DarkRed;
            ResultTitleText.Foreground = Brushes.DarkRed;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        // ================== Product AutoComplete Logic ==================
        private void ProductNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ProductNameTextBox == null || string.IsNullOrEmpty(ProductNameTextBox.Text))
            {
                ProductSuggestionsBorder.Visibility = Visibility.Collapsed;
                return;
            }

            var searchText = ProductNameTextBox.Text.Trim();
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
                ProductSuggestionsListBox.ItemsSource = filtered;
                ProductSuggestionsBorder.Visibility = Visibility.Visible;
            }
            else
            {
                ProductSuggestionsBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void ProductNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ProductNameTextBox.Text))
            {
                ProductNameTextBox_TextChanged(sender, null);
            }
        }

        private void ProductNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var focusedElement = FocusManager.GetFocusedElement(this);
            if (focusedElement != ProductSuggestionsListBox && focusedElement != ProductNameTextBox)
            {
                Task.Delay(150).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (!ProductSuggestionsListBox.IsMouseOver && !ProductNameTextBox.IsFocused)
                        {
                            ProductSuggestionsBorder.Visibility = Visibility.Collapsed;
                        }
                    });
                });
            }
        }

        private void ProductNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (ProductSuggestionsBorder.Visibility != Visibility.Visible)
                return;

            if (e.Key == Key.Down)
            {
                if (ProductSuggestionsListBox.Items.Count > 0)
                {
                    ProductSuggestionsListBox.Focus();
                    if (ProductSuggestionsListBox.SelectedIndex < ProductSuggestionsListBox.Items.Count - 1)
                        ProductSuggestionsListBox.SelectedIndex++;
                    else
                        ProductSuggestionsListBox.SelectedIndex = 0;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (ProductSuggestionsListBox.Items.Count > 0)
                {
                    ProductSuggestionsListBox.Focus();
                    if (ProductSuggestionsListBox.SelectedIndex > 0)
                        ProductSuggestionsListBox.SelectedIndex--;
                    else
                        ProductSuggestionsListBox.SelectedIndex = ProductSuggestionsListBox.Items.Count - 1;
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (ProductSuggestionsListBox.SelectedItem != null)
                {
                    SelectProductSuggestion(ProductSuggestionsListBox.SelectedItem.ToString());
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                ProductSuggestionsBorder.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void ProductSuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { }

        private void ProductSuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ProductSuggestionsListBox.SelectedItem != null)
            {
                SelectProductSuggestion(ProductSuggestionsListBox.SelectedItem.ToString());
            }
        }

        private void ProductSuggestionsListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void SelectProductSuggestion(string selectedProduct)
        {
            ProductNameTextBox.Text = selectedProduct;
            ProductSuggestionsBorder.Visibility = Visibility.Collapsed;
            ProductNameTextBox.Focus();
        }
    }
}
