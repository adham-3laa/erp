using erp.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace erp.Views.Inventory
{
    public partial class InventoryCheckPage : Page
    {
        private readonly InventoryService _service = new();

        public InventoryCheckPage()
        {
            InitializeComponent();
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
    }
}
