using erp.ViewModels;
using System.Windows;

namespace erp.Views.Users
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow(string currentUserId)
        {
            InitializeComponent();
            DataContext = new ChangePasswordViewModel(currentUserId);
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChangePasswordViewModel vm)
                vm.NewPassword = NewPasswordBox.Password;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChangePasswordViewModel vm)
                vm.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }
}
