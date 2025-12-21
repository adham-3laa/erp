using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace EduGate.Views.Orders
{
    public partial class CreateOrderPage : Page
    {
        // ObservableCollection عشان الـ ComboBox يحدث نفسه
        public ObservableCollection<ProductDto> Products { get; set; }

        private readonly HttpClient _httpClient;

        public CreateOrderPage()
        {
            InitializeComponent();
            OrdersTopBarControl.ApprovedOrdersClicked += (_, __) =>
                NavigationService.Navigate(new ApprovedOrdersPage());

            OrdersTopBarControl.SalesRepOrdersClicked += (_, __) =>
                NavigationService.Navigate(new SalesRepOrdersPage());
        }


        // تحميل المنتجات أول ما الصفحة تفتح
        private async void CreateOrderPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                var products = await _httpClient.GetFromJsonAsync<ProductDto[]>("api/products");

                Products.Clear();
                foreach (var product in products)
                    Products.Add(product);
            }
            catch (Exception ex)
            {
                MessageBox.Show("حصل خطأ أثناء تحميل المنتجات\n" + ex.Message);
            }
        }

        // السماح بالأرقام فقط في الكمية
        private void Quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }

        // التحقق بعد ما يسيب خانة الكمية
        private void Quantity_LostFocus(object sender, RoutedEventArgs e)
        {
            if (ProductComboBox.SelectedItem is not ProductDto selectedProduct)
                return;

            if (!int.TryParse(QuantityTextBox.Text, out int quantity))
            {
                QuantityTextBox.Text = "";
                return;
            }

            if (quantity <= 0)
            {
                MessageBox.Show("الكمية لازم تكون أكبر من صفر");
                QuantityTextBox.Text = "";
                return;
            }

            if (quantity > selectedProduct.AvailableQuantity)
            {
                MessageBox.Show($"الكمية المتاحة من المنتج هي {selectedProduct.AvailableQuantity}");
                QuantityTextBox.Text = selectedProduct.AvailableQuantity.ToString();
            }
        }

        // زر تأكيد الطلب
        private async void ConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CustomerNameTextBox.Text))
            {
                MessageBox.Show("من فضلك أدخل اسم العميل");
                return;
            }

            if (ProductComboBox.SelectedItem is not ProductDto selectedProduct)
            {
                MessageBox.Show("من فضلك اختر منتج");
                return;
            }

            if (!int.TryParse(QuantityTextBox.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("من فضلك أدخل كمية صحيحة");
                return;
            }

            if (quantity > selectedProduct.AvailableQuantity)
            {
                MessageBox.Show("الكمية المطلوبة أكبر من المتاح");
                return;
            }

            var orderRequest = new
            {
                CustomerName = CustomerNameTextBox.Text.Trim(),
                ProductId = selectedProduct.Id,
                Quantity = quantity
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/orders", orderRequest);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("تم إنشاء الطلب بنجاح ✅");
                    ClearForm();
                }
                else
                {
                    MessageBox.Show("فشل إنشاء الطلب");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("حصل خطأ\n" + ex.Message);
            }
        }

        private void ClearForm()
        {
            CustomerNameTextBox.Clear();
            ProductComboBox.SelectedIndex = -1;
            QuantityTextBox.Clear();
        }
    }

    // DTO
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AvailableQuantity { get; set; }
    }
}
