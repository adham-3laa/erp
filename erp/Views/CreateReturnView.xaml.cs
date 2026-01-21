using erp.Services;
using erp.ViewModels.Returns;
using erp;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace erp.Views
{
    public partial class CreateReturnView : Page
    {
        private readonly CreateReturnViewModel _viewModel;

        public CreateReturnView()
        {
            InitializeComponent();

            // Instantiate services locally or via App if available
            // Note: Services are lightweight API wrappers usually
            var returnsService = new ReturnsService(App.Api);
            var inventoryService = new InventoryService();

            _viewModel = new CreateReturnViewModel(returnsService, inventoryService);
            DataContext = _viewModel;
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private void SupplierNameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (!_viewModel.IsSupplierSuggestionOpen) return;

            switch (e.Key)
            {
                case Key.Down:
                    if (SupplierList.SelectedIndex < SupplierList.Items.Count - 1)
                    {
                        SupplierList.SelectedIndex++;
                        SupplierList.ScrollIntoView(SupplierList.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.Up:
                    if (SupplierList.SelectedIndex > 0)
                    {
                        SupplierList.SelectedIndex--;
                        SupplierList.ScrollIntoView(SupplierList.SelectedItem);
                    }
                    e.Handled = true;
                    break;

                case Key.Enter:
                    if (SupplierList.SelectedItem != null)
                    {
                        _viewModel.SelectedSupplierSuggestion = (string)SupplierList.SelectedItem;
                    } 
                    else if (SupplierList.Items.Count > 0)
                    {
                         // Optional: Select top if nothing selected
                         // _viewModel.SelectedSupplierSuggestion = (string)SupplierList.Items[0];
                    }
                    // If focusing logic needs adjustment:
                    // Keyboard.ClearFocus(); 
                    e.Handled = true;
                    break;

                case Key.Escape:
                    _viewModel.IsSupplierSuggestionOpen = false;
                    e.Handled = true;
                    break;
            }
        }
    }
}
