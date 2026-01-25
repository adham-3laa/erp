using erp.DTOS.InvoicesDTOS;
using erp.ViewModels;
using erp.ViewModels.Invoices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace erp.Views.Invoices
{
    public partial class InvoicesListPage : Page
    {
        public InvoicesListPage()
        {
            InitializeComponent();
            DataContext = new InvoicesListViewModel();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is InvoicesListViewModel vm)
                vm.LoadInvoicesCommand.Execute(null);
        }

        private void InvoicesGrid_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid grid &&
                grid.SelectedItem is InvoiceResponseDto invoice)
            {
                var nav = NavigationService.GetNavigationService(this);

                if (nav != null)
                    nav.Navigate(new InvoiceDetailsPage(invoice));
                else
                    MessageBox.Show("NavigationService غير متاح");
            }
        }

        // ✅ Autocomplete click
        private void RecipientSuggestion_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is TextBlock tb &&
                tb.DataContext is RecipientSuggestion item &&
                DataContext is InvoicesListViewModel vm)
            {
                vm.SelectRecipient(item);
            }
        }

        // ✅ Navigate to Sales & Return Invoice page
        private void SalesReturnInvoice_Click(object sender, RoutedEventArgs e)
        {
            var nav = NavigationService.GetNavigationService(this);

            if (nav != null)
                nav.Navigate(new SalesReturnInvoicePage());
            else
                MessageBox.Show("NavigationService غير متاح", "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
        }

    }
}
