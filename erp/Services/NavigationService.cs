using erp.Views.Users;
using System.Windows.Controls;

namespace erp.Services
{
    public static class NavigationService
    {
        private static Frame _mainFrame;

        public static void Initialize(Frame mainFrame)
        {
            _mainFrame = mainFrame;
        }

        public static void NavigateToUsers()
        {
            _mainFrame?.Navigate(new AllUsersPage());
        }

        public static void NavigateToCreateUser()
        {
            _mainFrame?.Navigate(new CreateUserPage());
        }

        public static void NavigateToUpdateUser(string userId)
        {
            _mainFrame?.Navigate(new UpdateUserPage(userId));
        }

        public static void NavigateToCurrentUser()
        {
            _mainFrame?.Navigate(new CurrentUserPage());
        }

        public static void NavigateBack()
        {
            if (_mainFrame?.CanGoBack == true)
                _mainFrame.GoBack();
        }
    }
}
