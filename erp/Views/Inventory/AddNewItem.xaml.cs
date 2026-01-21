using EduGate.Models;
using erp.Services;
using erp.DTOS;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

        // ✅ كل أسماء الموردين الموجودة في السيستم
        private List<string> _allSuppliers = new();

        private readonly UserService _userService = new UserService(App.Api);

        public AddNewItem()
        {
            InitializeComponent();
        }

        // زرار "إدخال عدد المنتجات"
        private async void ShowCount_Click(object sender, RoutedEventArgs e)
        {
            // تحميل قائمة الموردين عند فتح النموذج
            await LoadAllSuppliersAsync();
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

            // ✅ التحقق من أن اسم المورد موجود في قائمة الموردين
            var supplierName = SupplierNameTextBox.Text.Trim();
            if (!_allSuppliers.Contains(supplierName))
            {
                MessageBox.Show("هذا المورد غير موجود");
                return;
            }

            if (!int.TryParse(CountTextBox.Text, out _count) || _count <= 0)
            {
                MessageBox.Show("من فضلك أدخل رقم صحيح لعدد المنتجات");
                return;
            }

            _supplierName = supplierName;

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
        }

        private async Task LoadAllSuppliersAsync()
        {
            try
            {
                var response = await _userService.GetUsersAsync(
                    userType: "Supplier",
                    isActive: true,
                    page: 1,
                    pageSize: 1000);

                _allSuppliers = response?.Users
                    ?.Where(u => !string.IsNullOrWhiteSpace(u.Fullname))
                    .Select(u => u.Fullname.Trim())
                    .Distinct()
                    .ToList() ?? new List<string>();
            }
            catch
            {
                _allSuppliers = new List<string>();
            }
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

        // ================== AutoComplete Logic ==================
        private void CategoryTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CategoryTextBox == null || string.IsNullOrEmpty(CategoryTextBox.Text))
            {
                SuggestionsBorder.Visibility = Visibility.Collapsed;
                return;
            }

            var searchText = CategoryTextBox.Text.Trim();
            
            // ترتيب النتائج: المطابقة التامة أولاً، ثم التي تبدأ بالنص، ثم التي تحتويه
            var filtered = _allCategories
                .Where(c => c.Contains(searchText, System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(c =>
                {
                    if (c.Equals(searchText, System.StringComparison.OrdinalIgnoreCase))
                        return 0; // المطابقة التامة
                    if (c.StartsWith(searchText, System.StringComparison.OrdinalIgnoreCase))
                        return 1; // يبدأ بالنص
                    return 2; // يحتوي النص
                })
                .ThenBy(c => c.Length) // ثم ترتيب حسب الطول
                .Take(10) // عرض أول 10 اقتراحات فقط
                .ToList();

            if (filtered.Count > 0)
            {
                SuggestionsListBox.ItemsSource = filtered;
                SuggestionsBorder.Visibility = Visibility.Visible;
            }
            else
            {
                SuggestionsBorder.Visibility = Visibility.Collapsed;
            }
        }

        private void CategoryTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(CategoryTextBox.Text))
            {
                CategoryTextBox_TextChanged(sender, null);
            }
        }

        private void CategoryTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            // تأخير إخفاء الاقتراحات لتسمح بالنقر على العنصر
            var focusedElement = FocusManager.GetFocusedElement(this);
            if (focusedElement != SuggestionsListBox && focusedElement != CategoryTextBox)
            {
                Task.Delay(150).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (!SuggestionsListBox.IsMouseOver && !CategoryTextBox.IsFocused)
                        {
                            SuggestionsBorder.Visibility = Visibility.Collapsed;
                        }
                    });
                });
            }
        }

        private void CategoryTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (SuggestionsBorder.Visibility != Visibility.Visible)
                return;

            if (e.Key == Key.Down)
            {
                if (SuggestionsListBox.Items.Count > 0)
                {
                    SuggestionsListBox.Focus();
                    if (SuggestionsListBox.SelectedIndex < SuggestionsListBox.Items.Count - 1)
                    {
                        SuggestionsListBox.SelectedIndex++;
                    }
                    else
                    {
                        SuggestionsListBox.SelectedIndex = 0;
                    }
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (SuggestionsListBox.Items.Count > 0)
                {
                    SuggestionsListBox.Focus();
                    if (SuggestionsListBox.SelectedIndex > 0)
                    {
                        SuggestionsListBox.SelectedIndex--;
                    }
                    else
                    {
                        SuggestionsListBox.SelectedIndex = SuggestionsListBox.Items.Count - 1;
                    }
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Enter)
            {
                if (SuggestionsListBox.SelectedItem != null)
                {
                    SelectSuggestion(SuggestionsListBox.SelectedItem.ToString());
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Escape)
            {
                SuggestionsBorder.Visibility = Visibility.Collapsed;
                e.Handled = true;
            }
        }

        private void SuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // يمكن إضافة منطق إضافي هنا إذا لزم الأمر
        }

        private void SuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SuggestionsListBox.SelectedItem != null)
            {
                SelectSuggestion(SuggestionsListBox.SelectedItem.ToString());
            }
        }

        private void SuggestionsListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // منع فقدان التركيز عند النقر على الاقتراحات
            e.Handled = false;
        }

        private void SelectSuggestion(string selectedCategory)
        {
            if (DataContext is Product product)
            {
                product.Category = selectedCategory;
            }
            CategoryTextBox.Text = selectedCategory;
            SuggestionsBorder.Visibility = Visibility.Collapsed;
            CategoryTextBox.Focus();
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
            
            // ترتيب النتائج: المطابقة التامة أولاً، ثم التي تبدأ بالنص، ثم التي تحتويه
            var filtered = _allSuppliers
                .Where(s => s.Contains(searchText, System.StringComparison.OrdinalIgnoreCase))
                .OrderBy(s =>
                {
                    if (s.Equals(searchText, System.StringComparison.OrdinalIgnoreCase))
                        return 0; // المطابقة التامة
                    if (s.StartsWith(searchText, System.StringComparison.OrdinalIgnoreCase))
                        return 1; // يبدأ بالنص
                    return 2; // يحتوي النص
                })
                .ThenBy(s => s.Length) // ثم ترتيب حسب الطول
                .Take(10) // عرض أول 10 اقتراحات فقط
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
            // تأخير إخفاء الاقتراحات لتسمح بالنقر على العنصر
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
                    {
                        SupplierSuggestionsListBox.SelectedIndex++;
                    }
                    else
                    {
                        SupplierSuggestionsListBox.SelectedIndex = 0;
                    }
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Up)
            {
                if (SupplierSuggestionsListBox.Items.Count > 0)
                {
                    SupplierSuggestionsListBox.Focus();
                    if (SupplierSuggestionsListBox.SelectedIndex > 0)
                    {
                        SupplierSuggestionsListBox.SelectedIndex--;
                    }
                    else
                    {
                        SupplierSuggestionsListBox.SelectedIndex = SupplierSuggestionsListBox.Items.Count - 1;
                    }
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

        private void SupplierSuggestionsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // يمكن إضافة منطق إضافي هنا إذا لزم الأمر
        }

        private void SupplierSuggestionsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SupplierSuggestionsListBox.SelectedItem != null)
            {
                SelectSupplierSuggestion(SupplierSuggestionsListBox.SelectedItem.ToString());
            }
        }

        private void SupplierSuggestionsListBox_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            // منع فقدان التركيز عند النقر على الاقتراحات
            e.Handled = false;
        }

        private void SelectSupplierSuggestion(string selectedSupplier)
        {
            SupplierNameTextBox.Text = selectedSupplier;
            SupplierSuggestionsBorder.Visibility = Visibility.Collapsed;
            SupplierNameTextBox.Focus();
        }
    }
}
