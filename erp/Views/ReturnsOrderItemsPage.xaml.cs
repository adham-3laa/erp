using erp.ViewModels.Returns;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Returns
{
    public partial class ReturnsOrderItemsView : Page
    {
        private readonly ReturnsOrderItemsViewModel _vm;
        private readonly CreateReturnViewModel _createReturnVm;

        public ReturnsOrderItemsView(
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
            var orderId = OrderIdTextBoxInput.Text?.Trim();

            if (string.IsNullOrWhiteSpace(orderId))
            {
                MessageBox.Show("من فضلك أدخل رقم الطلب");
                return;
            }

            await _vm.LoadOrderItemsAsync(orderId);
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }

        private void CreateReturn_Click(object sender, RoutedEventArgs e)
        {
            var view = new CreateReturnView(_createReturnVm);
            NavigationService?.Navigate(view);
        }
    }
}
