using System.Windows;
using System.Windows.Controls;
using erp.ViewModels;

namespace erp.Views.Expenses
{
    public partial class ExpensesListPage : Page
    {
        public ExpensesListPage()
        {
            InitializeComponent();
            DataContext = new ExpensesListViewModel();
        }

        private void AddExpense_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this) as MainWindow;
            window?.MainFrame.Navigate(new AddExpensePage());
        }

        private void MyExpenses_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this) as MainWindow;
            window?.MainFrame.Navigate(new MyExpensesPage());
        }

        private void ExpensesByAccountant_Click(object sender, RoutedEventArgs e)
        {
            var accountantUserId = "acc-1001"; // الـ Id الثابت للمحاسب الوحيد
            var window = Window.GetWindow(this) as MainWindow;
            window?.MainFrame.Navigate(new ExpensesByAccountantPage(accountantUserId));
        }
        // ✅ زر التحديث
        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this) as MainWindow;
            window?.MainFrame.Navigate(new ExpensesListPage());
        }

    }
}
