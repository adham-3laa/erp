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

        // إضافة منتج جديد
        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_viewModel.CurrentProduct.ProductId) ||
                _viewModel.CurrentProduct.Quantity <= 0 ||
                string.IsNullOrWhiteSpace(_viewModel.CurrentProduct.Reason))
            {
                MessageBox.Show("من فضلك قم بإدخال جميع البيانات.");
                return;
            }

            _viewModel.AddProduct();

            // إعادة تهيئة المنتج الحالي (مهم جدًا)
            _viewModel.CurrentProduct = new CreateReturnItemDto();
        }
    }
}
