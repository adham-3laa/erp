using EduGate.Views.Inventory;
using erp.Views.Category;
using erp.Views.Expenses;
using erp.Views.Invoices;
using erp.Views.Users;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace erp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // تهيئة NavigationService
            erp.Services.NavigationService.Initialize(MainFrame);

            // افتح صفحة المستخدمين عند بدء التشغيل
            MainFrame.Content = null;
        }

        // ====== Navigation Methods ======

        public void NavigateToUsersPage()
        {
            MainFrame.Navigate(new AllUsersPage());
            SelectNavItem("Users");
        }

        public void NavigateToCurrentUser()
        {
            MainFrame.Navigate(new CurrentUserPage());
            SelectNavItem(null);
        }

        private void CurrentUserButton_Click(object sender, RoutedEventArgs e)
            => NavigateToCurrentUser();

        private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavListBox.SelectedItem is not ListBoxItem selectedItem)
                return;

            var tag = selectedItem.Tag as string;
            if (string.IsNullOrWhiteSpace(tag))
                return;

            switch (tag)
            {
                case "Users":
                    NavigateToUsersPage();
                    break;

                case "Inventory":
                    MainFrame.Navigate(new InventoryPage());
                    break;

                case "Invoices":
                    MainFrame.Navigate(new InvoicesListPage());
                    break;

                case "Expenses":
                    MainFrame.Navigate(new ExpensesListPage());
                    break;

                case "Items":
                    MainFrame.Navigate(new CategoryListPage());
                    break;

                // 👇 هنا تضيف التقارير
                case "Reports":
                    MainFrame.Navigate(new erp.Views.Reports.SalesReportPage());
                    break;

                case "Orders":
                case "Suppliers":
                case "Auth":
                default:
                    ShowUnderDevelopment(tag);
                    break;
            }
        }


        private static void ShowUnderDevelopment(string tag)
        {
            string pageName = tag switch
            {
                "Orders" => "الطلبات",
                "Suppliers" => "الموردين",
                "Auth" => "المصادقة",
                _ => "الصفحة"
            };

            MessageBox.Show($"صفحة {pageName} قيد التطوير", "تطوير");
        }

        private void SelectNavItem(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                NavListBox.SelectedItem = null;
                return;
            }

            foreach (ListBoxItem item in NavListBox.Items)
            {
                if ((item.Tag as string) == tag)
                {
                    NavListBox.SelectedItem = item;
                    return;
                }
            }
        }

        // ====== Window Controls ======

        private void Sidebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Double click toggles maximize
            if (e.ClickCount == 2)
            {
                Max_Click(sender, e);
                return;
            }

            if (e.ChangedButton == MouseButton.Left)
            {
                try { DragMove(); } catch { }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Min_Click(object sender, RoutedEventArgs e)
            => WindowState = WindowState.Minimized;
    }
}
