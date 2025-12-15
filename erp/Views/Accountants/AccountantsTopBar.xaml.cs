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

        private void Navigate(Page page)
        {
            if (Window.GetWindow(this) is MainWindow mw && mw.MainFrame != null)
                mw.MainFrame.Navigate(page);
        }

        private void AddAccountant_Click(object sender, RoutedEventArgs e)
            => Navigate(new AddAccountantPage());

        private void AllAccountants_Click(object sender, RoutedEventArgs e)
            => Navigate(new AllAccountantsPage());

        private void CurrentAccountant_Click(object sender, RoutedEventArgs e)
            => Navigate(new CurrentAccountantPage());
    }
}
