using erp.ViewModels.Returns;
using erp.DTOS;
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

        // زر الرجوع
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
                NavigationService.GoBack();
        }

        // إضافة منتج جديد
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (!_viewModel.IsCurrentProductValid())
            {
                MessageBox.Show("من فضلك قم بإدخال جميع بيانات المنتج.");
                return;
            }

            _viewModel.AddProduct();
            _viewModel.CurrentProduct = new CreateReturnItemDto();
        }
    }
}
