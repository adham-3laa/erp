using EduGate.Models;
using erp.Services;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Inventory
{
    public partial class AddNewItem : Page
    {
        private readonly InventoryService _service = new();
        private readonly List<Product> _products = new();

        private int _count;
        private int _index;

        // اسم المورد (بيتبعت للـ API كـ supplierName)
        private string _supplierName = "";

        // ✅ كل الأصناف الموجودة في السيستم
        private List<string> _allCategories = new();

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
        private async void StartWizard_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SupplierNameTextBox.Text))
            {
                MessageBox.Show("من فضلك أدخل اسم المورد");
                return;
            }

            if (!int.TryParse(CountTextBox.Text, out _count) || _count <= 0)
            {
                MessageBox.Show("من فضلك أدخل رقم صحيح لعدد المنتجات");
                return;
            }

            _supplierName = SupplierNameTextBox.Text.Trim();

            _products.Clear();
            for (int i = 0; i < _count; i++)
                _products.Add(new Product());

            // ✅ تحميل كل الأصناف من السيستم
            await LoadAllCategoriesAsync();

            CountPanel.Visibility = Visibility.Collapsed;
            FormPanel.Visibility = Visibility.Visible;

            _index = 0;
            LoadCurrent();
        }

        private async Task LoadAllCategoriesAsync()
        {
            var products = await _service.GetAllProductsLookupAsync();
            _allCategories = products
                .Select(p => p.CategoryName)
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Distinct()
                .ToList();

            // ✅ ربط ComboBox بالأصناف
            CategoryComboBox.ItemsSource = _allCategories;
        }


        private void LoadCurrent()
        {
            if (_products.Count == 0 || _index < 0 || _index >= _products.Count)
                return;

            DataContext = _products[_index];
            StepTitle.Text = $"المنتج {_index + 1} من {_count}";

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
                await _service.AddProductsWithCategoryNameAsync(_products, _supplierName);

                MessageBox.Show("تم إضافة المنتجات بنجاح ✅");
                NavigationService?.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // ================== Validation ==================
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

            // ✅ التحقق من الصنف
            if (!IsCategoryValid(product.Category))
                return false;

            return true;
        }

        private bool IsCategoryValid(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                MessageBox.Show("اسم الصنف مطلوب");
                return false;
            }

            if (!_allCategories.Contains(category))
            {
                MessageBox.Show($"اسم الصنف '{category}' غير موجود في النظام");
                return false;
            }

            return true;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }
    }
}
