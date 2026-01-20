using erp.DTOS.Cheques;
using erp.ViewModels.Cheques;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace erp.Views.Cheques
{
    public partial class EditChequePage : Page
    {
        private readonly EditChequeViewModel _viewModel;

        public EditChequePage(ChequeDto cheque)
        {
            InitializeComponent();
            _viewModel = new EditChequeViewModel(cheque);
            DataContext = _viewModel;
            _viewModel.OnSuccess += (s, e) => 
            {
               if (NavigationService.CanGoBack) NavigationService.GoBack();
            };
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack) NavigationService.GoBack();
        }
    }
}
