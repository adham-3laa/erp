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
            if (!int.TryParse(CountTextBox.Text, out _count) || _count <= 0)
            {
                MessageBox.Show("من فضلك أدخل رقم صحيح");
                return;
            }

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
            if (_index < _count - 1)
            {
                _index++;
                LoadCurrent();
            }
        }

        private void Prev_Click(object sender, RoutedEventArgs e)
        {
            if (_index > 0)
            {
                _index--;
                LoadCurrent();
            }
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                await _service.AddProductsWithCategoryNameAsync(_products);
                MessageBox.Show("تم إضافة المنتجات بنجاح ✅");
                NavigationService.GoBack();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
