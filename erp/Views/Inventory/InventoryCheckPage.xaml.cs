using EduGate.Services;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace EduGate.Views.Inventory
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

            if (string.IsNullOrWhiteSpace(ProductIdTextBox.Text))
            {
                ShowResultError("ProductId مطلوب");
                return;
            }

            if (!int.TryParse(ActualQuantityTextBox.Text, out int actualQty) || actualQty < 0)
            {
                ShowResultError("الكمية الفعلية غير صحيحة");
                return;
            }

            try
            {
                var result = await _service.AdjustInventoryAsync(
                    ProductIdTextBox.Text.Trim(),
                    actualQty);

                // إظهار الكارت
                ResultCard.Visibility = Visibility.Visible;

                OldQuantityText.Text = $"الكمية القديمة: {result.oldquantity}";
                NewQuantityText.Text = $"الكمية الجديدة: {result.newquantity}";
                FinancialImpactText.Text = result.financialimpact;

                // تحديد نوع النتيجة
                if (result.newquantity < result.oldquantity)
                {
                    int diff = result.oldquantity - result.newquantity;

                    ResultTitleText.Text = "❌ نتيجة الجرد: نقص";
                    ResultMessageText.Text = $"في نقص {diff} ";

                    ResultCard.BorderBrush = Brushes.Red;
                    ResultTitleText.Foreground = Brushes.Red;
                }
                else if (result.newquantity > result.oldquantity)
                {
                    int diff = result.newquantity - result.oldquantity;

                    ResultTitleText.Text = "✅ نتيجة الجرد: زيادة";
                    ResultMessageText.Text = $"في زيادة {diff} ";

                    ResultCard.BorderBrush = Brushes.Green;
                    ResultTitleText.Foreground = Brushes.Green;
                }
                else
                {
                    ResultTitleText.Text = "✔ نتيجة الجرد: متطابق";
                    ResultMessageText.Text = "لا يوجد فرق بين الكمية الفعلية والمخزنة";

                    ResultCard.BorderBrush = Brushes.Gray;
                    ResultTitleText.Foreground = Brushes.Gray;
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
            // يرجّعك للصفحة الأصلية (InventoryPage)
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }


    }


}
