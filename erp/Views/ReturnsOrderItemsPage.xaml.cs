using erp.ViewModels.Returns;
// using erp.Views.Returns;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace erp.Views
{
    public partial class ReturnsOrderItemsPage : Page
    {
        private readonly ReturnsOrderItemsViewModel _vm;
        private readonly CreateReturnViewModel _createReturnVm;

        public ReturnsOrderItemsPage(
            ReturnsOrderItemsViewModel vm,
            CreateReturnViewModel createReturnVm)
        {
            InitializeComponent();
            _vm = vm;
            _createReturnVm = createReturnVm;
            DataContext = _vm;
        }

        private async void Load_Click(object sender, RoutedEventArgs e)
        {
            await LoadOrderItemsAsync();
        }

        private async Task LoadOrderItemsAsync()
        {
            if (string.IsNullOrWhiteSpace(_vm.OrderId))
            {
                MessageBox.Show("من فضلك أدخل رقم الطلب", "تنبيه", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            await _vm.LoadOrderItemsAsync(_vm.OrderId.Trim());
        }

        private async void OrderIdTextBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                await LoadOrderItemsAsync();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void CreateReturn_Click(object sender, RoutedEventArgs e)
        {
            var view = new CreateReturnView();
            NavigationService?.Navigate(view);
        }
    }
}