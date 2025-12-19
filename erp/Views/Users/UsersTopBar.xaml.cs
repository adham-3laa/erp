using erp.Services;
using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Users
{
    public partial class UsersTopBar : UserControl
    {
        public UsersTopBar()
        {
            InitializeComponent();
        }

        private void AddUserButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToCreateUser();
        }

        private void ListUsersButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToUsers();
        }

        private void CurrentUserButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.NavigateToCurrentUser();
        }
    }
}