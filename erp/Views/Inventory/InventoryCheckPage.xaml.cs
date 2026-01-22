using erp.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace erp.Views.Inventory
{
    public partial class InventoryCheckPage : Page
    {
        private readonly InventoryService _inventoryService = new();
        private readonly OrdersService _ordersService;

        // Autocomplete suggestions
        private List<ProductAutocompleteItem> _productSuggestions = new();

        // Debounce timer
        private CancellationTokenSource? _productSearchCts;

        // المنتج المحدد
        private ProductAutocompleteItem? _selectedProduct;

        // ثابت لوقت التأخير في البحث (بالمللي ثانية)
        private const int SearchDebounceMs = 300;

        public InventoryCheckPage()
        {
            InitializeComponent();
            _ordersService = new OrdersService(App.Api);
            UpdatePlaceholders();
        }

        #region === Placeholders ===

        private void UpdatePlaceholders()
        {
            if (ProductPlaceholder != null)
                ProductPlaceholder.Visibility = string.IsNullOrEmpty(ProductNameTextBox?.Text)
                    ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region === Product Autocomplete ===

        private async void ProductNameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdatePlaceholders();
            ClearError(ProductErrorText, ProductInputWrapper);

            var searchText = ProductNameTextBox.Text?.Trim();
            if (string.IsNullOrWhiteSpace(searchText))
            {
                ProductSuggestionsPopup.IsOpen = false;
                _selectedProduct = null;
                return;
            }

            // إلغاء البحث السابق
            _productSearchCts?.Cancel();
            _productSearchCts = new CancellationTokenSource();
            var token = _productSearchCts.Token;

            try
            {
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
            if (!string.IsNullOrEmpty(ProductNameTextBox.Text) && _productSuggestions.Count > 0)
            {
                ProductSuggestionsPopup.IsOpen = true;
            }
        }

        private void ProductNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Task.Delay(200).ContinueWith(_ =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (!ProductSuggestionsListBox.IsMouseOver && !ProductNameTextBox.IsFocused)
                    {
                        ProductSuggestionsPopup.IsOpen = false;
                    }
                });
            });
        }

        private void ProductNameTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (!ProductSuggestionsPopup.IsOpen) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (ProductSuggestionsListBox.Items.Count > 0)
                    {
                        ProductSuggestionsListBox.Focus();
                        ProductSuggestionsListBox.SelectedIndex = Math.Min(
                            ProductSuggestionsListBox.SelectedIndex + 1,
                            ProductSuggestionsListBox.Items.Count - 1);
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (ProductSuggestionsListBox.Items.Count > 0)
                    {
                        ProductSuggestionsListBox.Focus();
                        ProductSuggestionsListBox.SelectedIndex = Math.Max(
                            ProductSuggestionsListBox.SelectedIndex - 1, 0);
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (ProductSuggestionsListBox.SelectedItem is ProductAutocompleteItem selected)
                    {
                        SelectProduct(selected);
                    }
                    e.Handled = true;
                    break;

                case Key.Escape:
                    ProductSuggestionsPopup.IsOpen = false;
                    e.Handled = true;
                    break;
            }
        }

        private void ProductSuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductSuggestionsListBox.SelectedItem is ProductAutocompleteItem selected)
            {
                SelectProduct(selected);
            }
        }

        private void ProductSuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (ProductSuggestionsListBox.SelectedItem is ProductAutocompleteItem selected)
            {
                SelectProduct(selected);
            }
        }

        private void ProductSuggestionsListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        private void SelectProduct(ProductAutocompleteItem product)
        {
            _selectedProduct = product;
            ProductNameTextBox.Text = product.name;
            ProductSuggestionsPopup.IsOpen = false;
            ProductNameTextBox.Focus();
            ProductNameTextBox.CaretIndex = ProductNameTextBox.Text.Length;
            UpdatePlaceholders();
        }

        #endregion

        #region === Input Validation ===

        private void QuantityTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        #endregion

        #region === Execute Adjustment ===

        private async void Adjust_Click(object sender, RoutedEventArgs e)
        {
            ClearAllErrors();
            ResultCard.Visibility = Visibility.Collapsed;

            // التحقق من اسم المنتج
            if (string.IsNullOrWhiteSpace(ProductNameTextBox.Text))
            {
                ShowError(ProductErrorText, ProductInputWrapper, "من فضلك أدخل اسم المنتج");
                ProductNameTextBox.Focus();
                return;
            }

            // التحقق من أن المنتج تم اختياره من القائمة
            var productName = ProductNameTextBox.Text.Trim();
            if (_selectedProduct == null || _selectedProduct.name.Trim() != productName)
            {
                ShowError(ProductErrorText, ProductInputWrapper, "يجب اختيار المنتج من القائمة المنسدلة");
                ProductNameTextBox.Focus();
                return;
            }

            // التحقق من الكمية
            if (!int.TryParse(ActualQuantityTextBox.Text, out int actualQty) || actualQty < 0)
            {
                ShowError(QuantityErrorText, QuantityInputWrapper, "الكمية الفعلية غير صحيحة (يجب أن تكون رقم صحيح 0 أو أكبر)");
                ActualQuantityTextBox.Focus();
                return;
            }

            bool updateStock = UpdateYesRadio.IsChecked == true;

            try
            {
                SetLoading(true);
                SetStatus("جاري تنفيذ الجرد...", StatusType.Loading);

                var result = await _inventoryService.AdjustInventoryByNameAsync(
                    productName,
                    actualQty,
                    updateStock);

                SetLoading(false);
                SetStatus(updateStock ? "تم تحديث المخزون بنجاح!" : "تم عرض النتيجة", StatusType.Success);

                // عرض النتيجة
                DisplayResult(result, actualQty, updateStock);
            }
            catch (Exception ex)
            {
                SetLoading(false);
                SetStatus("فشل في تنفيذ الجرد", StatusType.Error);
                ShowResultError("حدث خطأ أثناء تنفيذ الجرد:\n" + ex.Message);
            }
        }

        private void DisplayResult(dynamic result, int actualQty, bool updateStock)
        {
            ResultCard.Visibility = Visibility.Visible;

            // تحديد الكمية المسجلة في النظام
            int systemQty = updateStock ? result.oldquantity : result.systemquantity;

            // عرض الكميات
            SystemQuantityText.Text = systemQty.ToString();
            ActualQuantityResultText.Text = actualQty.ToString();

            // تحديد الفرق والنوع
            int difference = actualQty - systemQty;

            if (difference < 0)
            {
                // نقص
                ResultIconText.Text = "❌";
                ResultTitleText.Text = "نتيجة الجرد: نقص";
                ResultTitleText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                ResultSubtitleText.Text = $"يوجد نقص {Math.Abs(difference)} وحدة في المخزون";

                DifferenceArrowText.Text = "📉";
                DifferenceArrowText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));

                DifferenceBadge.Background = new SolidColorBrush(Color.FromRgb(254, 226, 226));
                DifferenceIconText.Text = "⬇️";
                DifferenceText.Text = $"- {Math.Abs(difference)} وحدة";
                DifferenceText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));

                ResultCard.Background = new SolidColorBrush(Color.FromRgb(254, 242, 242));
                ResultCard.BorderBrush = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                ResultCard.BorderThickness = new Thickness(2);

                SystemQuantityText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
                ActualQuantityResultText.Foreground = new SolidColorBrush(Color.FromRgb(220, 38, 38));
            }
            else if (difference > 0)
            {
                // زيادة
                ResultIconText.Text = "✅";
                ResultTitleText.Text = "نتيجة الجرد: زيادة";
                ResultTitleText.Foreground = new SolidColorBrush(Color.FromRgb(22, 163, 74));
                ResultSubtitleText.Text = $"يوجد زيادة {difference} وحدة في المخزون";

                DifferenceArrowText.Text = "📈";
                DifferenceArrowText.Foreground = new SolidColorBrush(Color.FromRgb(22, 163, 74));

                DifferenceBadge.Background = new SolidColorBrush(Color.FromRgb(220, 252, 231));
                DifferenceIconText.Text = "⬆️";
                DifferenceText.Text = $"+ {difference} وحدة";
                DifferenceText.Foreground = new SolidColorBrush(Color.FromRgb(22, 163, 74));

                ResultCard.Background = new SolidColorBrush(Color.FromRgb(240, 253, 244));
                ResultCard.BorderBrush = new SolidColorBrush(Color.FromRgb(22, 163, 74));
                ResultCard.BorderThickness = new Thickness(2);

                SystemQuantityText.Foreground = new SolidColorBrush(Color.FromRgb(22, 163, 74));
                ActualQuantityResultText.Foreground = new SolidColorBrush(Color.FromRgb(22, 163, 74));
            }
            else
            {
                // متطابق
                ResultIconText.Text = "✔️";
                ResultTitleText.Text = "نتيجة الجرد: متطابق";
                ResultTitleText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));
                ResultSubtitleText.Text = "الكمية الفعلية تتطابق مع المسجلة في النظام";

                DifferenceArrowText.Text = "➡️";
                DifferenceArrowText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));

                DifferenceBadge.Background = new SolidColorBrush(Color.FromRgb(243, 244, 246));
                DifferenceIconText.Text = "✓";
                DifferenceText.Text = "لا يوجد فرق";
                DifferenceText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));

                ResultCard.Background = new SolidColorBrush(Color.FromRgb(249, 250, 251));
                ResultCard.BorderBrush = new SolidColorBrush(Color.FromRgb(156, 163, 175));
                ResultCard.BorderThickness = new Thickness(2);

                SystemQuantityText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));
                ActualQuantityResultText.Foreground = new SolidColorBrush(Color.FromRgb(107, 114, 128));
            }

            // الأثر المالي
            FinancialImpactText.Text = result.financialimpact ?? "لا يوجد";

            // ملاحظة التحديث
            if (updateStock)
            {
                UpdateNoteText.Text = "✅ تم تحديث الكمية في النظام";
            }
            else
            {
                UpdateNoteText.Text = "⚠️ هذه معاينة فقط - لم يتم تحديث المخزون";
            }

            // Scroll to result
            ResultCard.BringIntoView();
        }

        private void ShowResultError(string message)
        {
            ResultCard.Visibility = Visibility.Visible;

            ResultIconText.Text = "❌";
            ResultTitleText.Text = "خطأ";
            ResultTitleText.Foreground = new SolidColorBrush(Color.FromRgb(185, 28, 28));
            ResultSubtitleText.Text = message;

            DifferenceArrowText.Text = "";
            SystemQuantityText.Text = "-";
            ActualQuantityResultText.Text = "-";

            DifferenceBadge.Visibility = Visibility.Collapsed;
            FinancialImpactBorder.Visibility = Visibility.Collapsed;
            UpdateNoteText.Text = "";

            ResultCard.Background = new SolidColorBrush(Color.FromRgb(254, 226, 226));
            ResultCard.BorderBrush = new SolidColorBrush(Color.FromRgb(185, 28, 28));
            ResultCard.BorderThickness = new Thickness(2);
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ProductNameTextBox.Clear();
            ActualQuantityTextBox.Text = "0";
            _selectedProduct = null;

            ResultCard.Visibility = Visibility.Collapsed;
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
            ClearError(ProductErrorText, ProductInputWrapper);
            ClearError(QuantityErrorText, QuantityInputWrapper);

            // Reset result card visibility flags
            DifferenceBadge.Visibility = Visibility.Visible;
            FinancialImpactBorder.Visibility = Visibility.Visible;
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
            AdjustButton.IsEnabled = !isLoading;
            ClearButton.IsEnabled = !isLoading;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        #endregion
    }
}
