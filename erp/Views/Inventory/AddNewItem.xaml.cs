using EduGate.Models;
using EduGate.Services;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace EduGate.Views.Inventory
{
    public partial class AddNewItem : Page
    {
        private readonly InventoryService _service = new();
        private readonly List<Product> _products = new();

        private int _count;
        private int _index;

        // SupplierId بيتحدد مرة واحدة
        private string _supplierId = "";

        public AddNewItem()
        {
            InitializeComponent();
        }

        // زرار "إدخال عدد المنتجات"
        private void ShowCount_Click(object sender, RoutedEventArgs e)
        {
            IntroPanel.Visibility = Visibility.Collapsed;
            CountPanel.Visibility = Visibility.Visible;
        }

        // زرار "ابدأ"
        private void StartWizard_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SupplierIdTextBox.Text))
            {
                MessageBox.Show("من فضلك أدخل SupplierId");
                return;
            }

            if (!int.TryParse(CountTextBox.Text, out _count) || _count <= 0)
            {
                MessageBox.Show("من فضلك أدخل رقم صحيح لعدد المنتجات");
                return;
            }

            _supplierId = SupplierIdTextBox.Text.Trim();

            _products.Clear();
            for (int i = 0; i < _count; i++)
                _products.Add(new Product());

            CountPanel.Visibility = Visibility.Collapsed;
            FormPanel.Visibility = Visibility.Visible;

            _index = 0;
            LoadCurrent();
        }

        private void LoadCurrent()
        {
            if (_products.Count == 0 || _index < 0 || _index >= _products.Count)
                return;

            DataContext = _products[_index];
            StepTitle.Text = $"المنتج {_index + 1} من {_count}";

            NextBtn.Visibility = _index == _count - 1
                ? Visibility.Collapsed
                : Visibility.Visible;

            SaveBtn.Visibility = _index == _count - 1
                ? Visibility.Visible
                : Visibility.Collapsed;
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
            if (_products.Count == 0)
                return;

            if (_index > 0)
            {
                _index--;
                LoadCurrent();
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
                await _service.AddProductsWithCategoryNameAsync(_products, _supplierId);
                MessageBox.Show("تم إضافة المنتجات بنجاح ✅");
                NavigationService.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ================== Validation (آمنة 100%) ==================
        private bool IsProductValid(Product product)
        {
            if (product == null)
            {
                MessageBox.Show("بيانات المنتج غير صالحة");
                return false;
            }

            if (string.IsNullOrWhiteSpace(product.Name))
            {
                MessageBox.Show("اسم المنتج مطلوب");
                return false;
            }

            if (product.SalePrice <= 0)
            {
                MessageBox.Show("سعر البيع غير صالح");
                return false;
            }

            if (product.BuyPrice <= 0)
            {
                MessageBox.Show("سعر الشراء غير صالح");
                return false;
            }

            if (product.Quantity <= 0)
            {
                MessageBox.Show("الكمية غير صالحة");
                return false;
            }

            if (string.IsNullOrWhiteSpace(product.Category))
            {
                MessageBox.Show("اسم الصنف مطلوب");
                return false;
            }

            return true;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // يرجّعك للصفحة اللي قبلها (InventoryPage)
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

    }
}
