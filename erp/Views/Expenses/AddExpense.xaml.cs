using System.Windows;
using System.Windows.Controls;
using erp.ViewModels;

namespace erp.Views.Expenses
{
    public partial class AddExpensePage : Page
    {
        private readonly ExpensesListViewModel _listVm;

        public AddExpensePage(ExpensesListViewModel listVm = null)
        {
            InitializeComponent();
            DataContext = new AddExpenseViewModel();
            _listVm = listVm;
        }

        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            // تحديث قائمة المصروفات إذا تم تمرير ViewModel
            if (_listVm != null)
            {
                await _listVm.LoadAllCommand.ExecuteAsync(null);
            }

            // العودة للصفحة السابقة إذا ممكن
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                // إذا مفيش صفحة سابقة، نروح لقائمة المصروفات
                var mainFrame = Application.Current.MainWindow?.FindName("MainFrame") as Frame;
                mainFrame?.Navigate(new ExpensesListPage());
            }
        }
    }
}
