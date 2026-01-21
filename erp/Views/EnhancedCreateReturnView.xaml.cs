using erp.Services;
using erp.ViewModels.Returns;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace erp.Views
{
    /// <summary>
    /// Enhanced Returns View with step-by-step flow and improved UX
    /// </summary>
    public partial class EnhancedCreateReturnView : Page
    {
        private readonly EnhancedCreateReturnViewModel _viewModel;

        public EnhancedCreateReturnView()
        {
            InitializeComponent();

            try
            {
                // Initialize services
                var returnsService = new ReturnsService(App.Api);
                var inventoryService = new InventoryService();

                // Initialize ViewModel
                _viewModel = new EnhancedCreateReturnViewModel(returnsService, inventoryService);
                DataContext = _viewModel;
            }
            catch (System.Exception ex)
            {
                ErrorHandlingService.LogError(ex, "EnhancedCreateReturnView initialization");
                ErrorHandlingService.ShowError("حدث خطأ أثناء تحميل صفحة المرتجعات");
            }
        }

        /// <summary>
        /// Navigate back to previous page
        /// </summary>
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        /// <summary>
        /// Handle keyboard navigation for supplier autocomplete
        /// </summary>
        private void SupplierNameBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_viewModel == null || !_viewModel.IsSupplierSuggestionOpen) 
                return;

            switch (e.Key)
            {
                case Key.Down:
                    if (SupplierList.SelectedIndex < SupplierList.Items.Count - 1)
                    {
                        SupplierList.SelectedIndex++;
                        SupplierList.ScrollIntoView(SupplierList.SelectedItem);
                    }
                    else if (SupplierList.Items.Count > 0 && SupplierList.SelectedIndex == -1)
                    {
                        SupplierList.SelectedIndex = 0;
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
                        // Select first item if nothing selected
                        _viewModel.SelectedSupplierSuggestion = (string)SupplierList.Items[0];
                    }
                    e.Handled = true;
                    break;

                case Key.Escape:
                    _viewModel.IsSupplierSuggestionOpen = false;
                    e.Handled = true;
                    break;

                case Key.Tab:
                    if (SupplierList.SelectedItem != null)
                    {
                        _viewModel.SelectedSupplierSuggestion = (string)SupplierList.SelectedItem;
                    }
                    _viewModel.IsSupplierSuggestionOpen = false;
                    break;
            }
        }
    }
}
