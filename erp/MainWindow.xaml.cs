using EduGate.Views.Inventory;
using erp.Services;
using erp.ViewModels.Returns;
using erp.Views.Category;
using erp.Views.Dashboard;     // ✅ NEW
using erp.Views.Expenses;
using erp.Views.Invoices;
using erp.Views.Payments;
using erp.Views.Returns;
using erp.Views.Users;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;



namespace erp
{
    public partial class MainWindow : Window
    {
        private readonly ApiClient _apiClient;


        public MainWindow()
        {
            InitializeComponent();

            var httpClient = ApiClient.CreateHttpClient();
            _apiClient = new ApiClient(httpClient, null);

            erp.Services.NavigationService.Initialize(MainFrame);
            NavigateToDashboard();
        }


        // ====== Navigation Methods ======

        public void NavigateToDashboard()
        {
            MainFrame.Navigate(new DashboardPage());
            SelectNavItem("Dashboard");
        }

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
                case "Dashboard":
                    NavigateToDashboard();
                    break;

                case "Users":
                    NavigateToUsersPage();
                    break;

                case "Inventory":
                    MainFrame.Navigate(new InventoryPage());
                    break;

                case "Payments":
                    MainFrame.Navigate(new PaymentsMenuPage());
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
                case "Returns":
                    {
                        var returnsService = new ReturnsService(_apiClient);
                        var vm = new ReturnsOrderItemsViewModel(returnsService);

                        MainFrame.Navigate(new ReturnsOrderItemsView(vm));
                        break;
                    }








                // 👇 هنا تضيف التقارير
                case "Reports":
                    MainFrame.Navigate(new erp.Views.Reports.SalesReportPage());
                    break;

                case "Orders":
                    MainFrame.Navigate(new EduGate.Views.Orders.ApprovedOrdersPage());
                    SelectNavItem("Orders");
                    break;

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
                "Dashboard" => "الداشبورد",
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
