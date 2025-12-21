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
    public partial class ExpensesByAccountantPage : Page
    {
        public ExpensesByAccountantPage()
        {
            InitializeComponent();
            DataContext = new ExpensesByAccountantViewModel();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var frame = Application.Current.MainWindow
                .FindName("MainFrame") as Frame;

            frame?.Navigate(new ExpensesListPage());
        }
    }
}
