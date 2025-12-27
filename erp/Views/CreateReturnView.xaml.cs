using erp.ViewModels.Returns;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Returns
{
    public partial class CreateReturnView : Page
    {
        private readonly CreateReturnViewModel _viewModel;

        public CreateReturnView(CreateReturnViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            DataContext = _viewModel;
        }

        private async void Submit_Click(object sender, RoutedEventArgs e)
        {
            var success = await _viewModel.SubmitReturnAsync();

            if (success)
            {
                MessageBox.Show("تم إرسال طلب المرتجع بنجاح ✅",
                                "نجاح",
                                MessageBoxButton.OK,
                                MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("لم يتم إرسال المرتجع ⚠️\nتأكد من إدخال كميات مرتجعة.",
                                "تنبيه",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }
    }
}
