using System.Windows;
using System.Windows.Controls;
using EduGate.Views.Accountants;

namespace EduGate
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            NavListBox.SelectionChanged += NavListBox_SelectionChanged;

            // صفحة افتراضية
            NavigateTo("Accountants");
        }

        private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavListBox.SelectedItem is not ListBoxItem item) return;

            var section = item.Tag?.ToString();
            NavigateTo(section);

            NavListBox.SelectedItem = null;
        }

        private void NavigateTo(string? section)
        {
            AccountantsTopBarControl.Visibility = Visibility.Collapsed;
            PageTitleText.Text = "لوحة التحكم";

            switch (section)
            {
                case "Accountants":
                    AccountantsTopBarControl.Visibility = Visibility.Visible;
                    PageTitleText.Text = "المحاسبين";
                    MainFrame.Navigate(new AllAccountantsPage());
                    break;

                case "Users":
                    PageTitleText.Text = "المستخدمين";
                    MainFrame.Content = null;
                    break;

                case "Inventory":
                    PageTitleText.Text = "المخزون";
                    MainFrame.Content = null;
                    break;

                case "Invoices":
                    PageTitleText.Text = "الفواتير";
                    MainFrame.Content = null;
                    break;

                case "Orders":
                    PageTitleText.Text = "الطلبات";
                    MainFrame.Content = null;
                    break;

                case "Expenses":
                    PageTitleText.Text = "المصروفات";
                    MainFrame.Content = null;
                    break;

                case "Categories":
                    PageTitleText.Text = "الأصناف";
                    MainFrame.Content = null;
                    break;

                case "Suppliers":
                    PageTitleText.Text = "الموردين";
                    MainFrame.Content = null;
                    break;

                case "Auth":
                    PageTitleText.Text = "المصادقة";
                    MainFrame.Content = null;
                    break;

                default:
                    MainFrame.Content = null;
                    break;
            }
        }

        // --- Window chrome (because WindowStyle=None) ---
        private void Window_DragMove(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                DragMove();
        }

        private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Maximize_Click(object sender, RoutedEventArgs e)
            => WindowState = (WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
