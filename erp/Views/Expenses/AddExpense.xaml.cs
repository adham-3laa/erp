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
    public partial class AddExpensePage : Page
    {
        public AddExpensePage()
        {
            InitializeComponent();
            DataContext = new AddExpenseViewModel();
        }

        private void BackToList_Click(object sender, RoutedEventArgs e)
        {
            var mainWindow = Application.Current.MainWindow as EduGate.MainWindow;

            // رجوع لصفحة قائمة المصروفات
            mainWindow?.MainFrame.Navigate(new ExpensesListPage());
        }
    }
}

