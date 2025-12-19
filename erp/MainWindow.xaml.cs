using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using erp.Views.Users;

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
            NavigateToUsersPage();
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

        private void NavListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (NavListBox.SelectedItem is ListBoxItem selectedItem)
            {
                string tag = selectedItem.Tag as string;

                if (tag == "Users")
                {
                    NavigateToUsersPage();
                }
                else
                {
                    // بقية الصفحات...
                    string pageName = tag switch
                    {
                        "Inventory" => "المخزون",
                        "Invoices" => "الفواتير",
                        "Orders" => "الطلبات",
                        "Expenses" => "المصروفات",
                        "Items" => "الأصناف",
                        "Suppliers" => "الموردين",
                        "Auth" => "المصادقة",
                        _ => "الصفحة"
                    };
                    MessageBox.Show($"صفحة {pageName} قيد التطوير", "تطوير");
                }
            }
        }

        private void CurrentUserButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToCurrentUser();
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
                if (item.Tag as string == tag)
                {
                    NavListBox.SelectedItem = item;
                    return;
                }
            }
        }

        // ====== Window Controls ======

        private void Sidebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Max_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
        }

        private void Min_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }
    }
}