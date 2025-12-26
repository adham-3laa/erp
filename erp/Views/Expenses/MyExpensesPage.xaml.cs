using erp.ViewModels;
using System.Windows.Controls;

namespace erp.Views.Expenses
{
    public partial class MyExpensesPage : Page
    {
        private readonly ExpensesListViewModel _listVm;

        public MyExpensesPage(ExpensesListViewModel listVm = null)
        {
            InitializeComponent();
            DataContext = new MyExpensesViewModel();
            _listVm = listVm;
        }

        private async void Back_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_listVm != null)
            {
                await _listVm.LoadAllCommand.ExecuteAsync(null);
            }

            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                var frame = System.Windows.Application.Current.MainWindow.FindName("MainFrame") as Frame;
                frame?.Navigate(new ExpensesListPage());
            }
        }
    }
}
