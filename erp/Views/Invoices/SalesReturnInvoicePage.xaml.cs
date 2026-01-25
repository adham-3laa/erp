using erp.ViewModels.Invoices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace erp.Views.Invoices
{
    /// <summary>
    /// Code-behind for SalesReturnInvoicePage.xaml
    /// 
    /// This page provides a comprehensive view of Sales & Return invoices
    /// (netting invoice) for a specific customer/partner.
    /// </summary>
    public partial class SalesReturnInvoicePage : Page
    {
        public SalesReturnInvoicePage()
        {
            InitializeComponent();
            DataContext = new SalesReturnInvoiceViewModel();
        }

        /// <summary>
        /// Called when the page is loaded
        /// </summary>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus on the partner search box for immediate input
            PartnerSearchBox?.Focus();
        }

        /// <summary>
        /// Navigate back to the invoices list page
        /// </summary>
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            var nav = NavigationService.GetNavigationService(this);
            if (nav != null && nav.CanGoBack)
            {
                nav.GoBack();
            }
            else
            {
                // Fallback: navigate to InvoicesListPage
                nav?.Navigate(new InvoicesListPage());
            }
        }

        /// <summary>
        /// Handle partner suggestion selection from autocomplete dropdown
        /// </summary>
        private void PartnerSuggestion_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element &&
                element.DataContext is PartnerSuggestion suggestion &&
                DataContext is SalesReturnInvoiceViewModel vm)
            {
                vm.SelectPartner(suggestion);
                
                // Close the popup explicitly
                SuggestionsPopup.IsOpen = false;
            }
        }
    }
}
