using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
            // التنقل لصفحة إضافة مصروف داخل MainFrame
            var frame = System.Windows.Application.Current.MainWindow
                        .FindName("MainFrame") as Frame;

            frame?.Navigate(new AddExpensePage());
        }
        private void MyExpenses_Click(object sender, RoutedEventArgs e)
        {
            var frame = System.Windows.Application.Current.MainWindow
                .FindName("MainFrame") as Frame;

            frame?.Navigate(new MyExpensesPage());
        }
        private void ExpensesByAccountant_Click(object sender, RoutedEventArgs e)
        {
            var frame = System.Windows.Application.Current.MainWindow
                .FindName("MainFrame") as Frame;

            frame?.Navigate(new ExpensesByAccountantPage());
        }
    }
}

