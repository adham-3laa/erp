using System.Windows;
using System.Windows.Controls;
using erp.ViewModels;

namespace erp.Views.Expenses
{
    public partial class ExpensesByAccountantPage : Page
    {
        private readonly ExpensesListViewModel _listVm;

        public ExpensesByAccountantPage(string accountantUserId, ExpensesListViewModel listVm = null)
        {
            InitializeComponent();
            DataContext = new ExpensesByAccountantViewModel(accountantUserId);

            _listVm = listVm; // ممكن تمرر ViewModel الصفحة السابقة لتحديثها عند الرجوع
        }

        private async void Back_Click(object sender, RoutedEventArgs e)
        {
            // لو في ViewModel للقائمة، حدثها قبل الرجوع
            if (_listVm != null)
            {
                await _listVm.LoadAllCommand.ExecuteAsync(null);
            }

            // رجوع للصفحة السابقة
            if (NavigationService != null && NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }
    }
}
