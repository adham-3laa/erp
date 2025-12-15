using erp.Views.Accountants;
using System.Windows;
using System.Windows.Controls;

namespace EduGate.Views.Accountants
{
    public partial class AccountantsTopBar : UserControl
    {
        public AccountantsTopBar()
        {
            InitializeComponent();
        }

        private void AddAccountant_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.MainFrame.Navigate(new AddAccountantPage());
        }

        private void AllAccountants_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.MainFrame.Navigate(new AllAccountantsPage());
        }

        private void CurrentAccountant_Click(object sender, RoutedEventArgs e)
        {
            var parentWindow = Window.GetWindow(this) as MainWindow;
            parentWindow?.MainFrame.Navigate(new CurrentAccountantPage());
        }
    }
}
