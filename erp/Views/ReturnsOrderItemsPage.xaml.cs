using System.Windows;
using System.Windows.Controls;
using erp.ViewModels.Returns;

namespace erp.Views.Returns
{
    public partial class ReturnsOrderItemsView : Page
    {
        private readonly ReturnsOrderItemsViewModel _vm;

        public ReturnsOrderItemsView(ReturnsOrderItemsViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = vm;
        }

        private async void Load_Click(object sender, RoutedEventArgs e)
        {
            var orderId = OrderIdTextBox.Text?.Trim();

            if (string.IsNullOrWhiteSpace(orderId))
            {
                MessageBox.Show("Please enter Order Id");
                return;
            }

            await _vm.LoadOrderItemsAsync(orderId);
        }
    }
}
