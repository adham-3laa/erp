using EduGate.Models;
using erp.Services;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace erp.Views.Inventory
{
    public partial class EditProductPage : Page
    {
        private readonly InventoryService _inventoryService;
        private readonly Product _product;
        private bool _isSaving = false;

        // Border gradient brushes for validation states
        private readonly LinearGradientBrush _normalBorderBrush;
        private readonly LinearGradientBrush _errorBorderBrush;

        public EditProductPage(Product product)
        {
            InitializeComponent();

            _inventoryService = new InventoryService();
            _product = product;

            // Initialize border brushes
            _normalBorderBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(229, 231, 235), 0),
                    new GradientStop(Color.FromRgb(209, 213, 219), 1)
                }
            };

            _errorBorderBrush = new LinearGradientBrush
            {
                StartPoint = new Point(0, 0),
                EndPoint = new Point(1, 1),
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Color.FromRgb(252, 165, 165), 0),
                    new GradientStop(Color.FromRgb(239, 68, 68), 1)
                }
            };

            DataContext = _product;

            // Initialize form values
            LoadProductData();
        }

        private void LoadProductData()
        {
            // Set header info
            ProductNameHeader.Text = _product.Name;
            ProductCodeText.Text = $"كود: {_product.code}";

            // Load values into textboxes
            NameTextBox.Text = _product.Name;
            SalePriceTextBox.Text = _product.SalePrice.ToString();
            BuyPriceTextBox.Text = _product.BuyPrice.ToString();
            QuantityTextBox.Text = _product.Quantity.ToString();
            CategoryTextBox.Text = _product.Category;
            DescriptionTextBox.Text = _product.Description;
        }

        #region Validation Methods

        private void NameTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateNameField();
            HideMessages();
        }

        private void CategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateCategoryField();
            HideMessages();
        }

        private void ValidateNumericField(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                if (textBox == SalePriceTextBox)
                    ValidateSalePriceField();
                else if (textBox == BuyPriceTextBox)
                    ValidateBuyPriceField();
                else if (textBox == QuantityTextBox)
                    ValidateQuantityField();
            }
            HideMessages();
        }

        private void NumericTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Allow only digits
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private bool ValidateNameField()
        {
            bool isValid = !string.IsNullOrWhiteSpace(NameTextBox.Text);
            SetFieldValidationState(NameInputWrapper, NameErrorText, isValid);
            return isValid;
        }

        private bool ValidateSalePriceField()
        {
            bool isValid = int.TryParse(SalePriceTextBox.Text, out int price) && price > 0;
            SetFieldValidationState(SalePriceInputWrapper, SalePriceErrorText, isValid);

            // Additional validation: sale price should be >= buy price
            if (isValid && int.TryParse(BuyPriceTextBox.Text, out int buyPrice) && price < buyPrice)
            {
                SalePriceErrorText.Text = "⚠️ سعر البيع يجب أن يكون أكبر من أو يساوي سعر الشراء";
                SetFieldValidationState(SalePriceInputWrapper, SalePriceErrorText, false);
                return false;
            }

            SalePriceErrorText.Text = "⚠️ سعر البيع غير صالح";
            return isValid;
        }

        private bool ValidateBuyPriceField()
        {
            bool isValid = int.TryParse(BuyPriceTextBox.Text, out int price) && price > 0;
            SetFieldValidationState(BuyPriceInputWrapper, BuyPriceErrorText, isValid);
            return isValid;
        }

        private bool ValidateQuantityField()
        {
            bool isValid = int.TryParse(QuantityTextBox.Text, out int quantity) && quantity >= 0;
            SetFieldValidationState(QuantityInputWrapper, QuantityErrorText, isValid);
            return isValid;
        }

        private bool ValidateCategoryField()
        {
            bool isValid = !string.IsNullOrWhiteSpace(CategoryTextBox.Text);
            SetFieldValidationState(CategoryInputWrapper, CategoryErrorText, isValid);
            return isValid;
        }

        private void SetFieldValidationState(Border wrapper, TextBlock errorText, bool isValid)
        {
            if (isValid)
            {
                wrapper.Background = _normalBorderBrush;
                errorText.Visibility = Visibility.Collapsed;
            }
            else
            {
                wrapper.Background = _errorBorderBrush;
                errorText.Visibility = Visibility.Visible;
            }
        }

        private bool ValidateAllFields()
        {
            bool isNameValid = ValidateNameField();
            bool isSalePriceValid = ValidateSalePriceField();
            bool isBuyPriceValid = ValidateBuyPriceField();
            bool isQuantityValid = ValidateQuantityField();
            bool isCategoryValid = ValidateCategoryField();

            return isNameValid && isSalePriceValid && isBuyPriceValid && isQuantityValid && isCategoryValid;
        }

        private void ResetAllValidationStates()
        {
            SetFieldValidationState(NameInputWrapper, NameErrorText, true);
            SetFieldValidationState(SalePriceInputWrapper, SalePriceErrorText, true);
            SetFieldValidationState(BuyPriceInputWrapper, BuyPriceErrorText, true);
            SetFieldValidationState(QuantityInputWrapper, QuantityErrorText, true);
            SetFieldValidationState(CategoryInputWrapper, CategoryErrorText, true);
        }

        #endregion

        #region UI State Methods

        private void SetLoadingState(bool isLoading)
        {
            _isSaving = isLoading;
            LoadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            SaveBtn.IsEnabled = !isLoading;
            CancelBtn.IsEnabled = !isLoading;
        }

        private void ShowSuccessMessage(string message)
        {
            SuccessMessageText.Text = message;
            SuccessMessageBorder.Visibility = Visibility.Visible;
            GeneralErrorBorder.Visibility = Visibility.Collapsed;
        }

        private void ShowErrorMessage(string message)
        {
            GeneralErrorText.Text = message;
            GeneralErrorBorder.Visibility = Visibility.Visible;
            SuccessMessageBorder.Visibility = Visibility.Collapsed;
        }

        private void HideMessages()
        {
            SuccessMessageBorder.Visibility = Visibility.Collapsed;
            GeneralErrorBorder.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region Event Handlers

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (_isSaving) return;

            // Validate all fields
            if (!ValidateAllFields())
            {
                ShowErrorMessage("يرجى تصحيح الأخطاء في الحقول المطلوبة");
                return;
            }

            try
            {
                SetLoadingState(true);
                HideMessages();

                // Parse and update product values
                _product.Name = NameTextBox.Text.Trim();
                _product.SalePrice = int.Parse(SalePriceTextBox.Text);
                _product.BuyPrice = int.Parse(BuyPriceTextBox.Text);
                _product.Quantity = int.Parse(QuantityTextBox.Text);
                _product.Category = CategoryTextBox.Text.Trim();
                _product.Description = DescriptionTextBox.Text?.Trim() ?? "";

                // Save to server
                await _inventoryService.UpdateProductAsync(_product);

                // Update header with new name
                ProductNameHeader.Text = _product.Name;

                ShowSuccessMessage("تم حفظ التعديلات بنجاح ✅");

                // Wait a moment then go back
                await System.Threading.Tasks.Task.Delay(1200);
                
                if (NavigationService?.CanGoBack == true)
                    NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                ShowErrorMessage($"حدث خطأ أثناء الحفظ: {ex.Message}");
            }
            finally
            {
                SetLoadingState(false);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (_isSaving)
            {
                var result = MessageBox.Show(
                    "جاري حفظ البيانات، هل تريد الإلغاء والعودة؟",
                    "تأكيد",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (result != MessageBoxResult.Yes)
                    return;
            }

            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        #endregion
    }
}
