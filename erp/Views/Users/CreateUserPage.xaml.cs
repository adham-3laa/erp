using System.Windows;
using System.Windows.Controls;

namespace erp.Views.Users
{
    public partial class CreateUserPage : UserControl
    {
        public CreateUserPage()
        {
            InitializeComponent();
            DataContext = new ViewModels.CreateUserViewModel();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.CreateUserViewModel vm)
                vm.Password = PasswordBox.Password;
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ViewModels.CreateUserViewModel vm)
                vm.ConfirmPassword = ConfirmPasswordBox.Password;
        }
    }

}