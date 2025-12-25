using erp.ViewModels;
using erp.Services;
using System.Windows;

namespace erp.Views.Users
{
    public partial class ChangePasswordWindow : Window
    {
        public ChangePasswordWindow(string userId, IAuthSession session)
        {
            InitializeComponent();

            var vm = new ChangePasswordViewModel(userId, session);
            vm.CloseAction = Close;
            DataContext = vm;
        }

        // ✅ دي كانت ناقصة
        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is ChangePasswordViewModel vm &&
                sender is System.Windows.Controls.PasswordBox pb)
            {
                vm.NewPassword = pb.Password;
            }
        }
    }
}
