using System.Windows;
using erp.ViewModels.Auth;

namespace erp.Views.Auth
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel(App.Auth, OnLoginSuccess);
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = Pwd.Password;
                if (vm.LoginCommand.CanExecute(null))
                    vm.LoginCommand.Execute(null);
            }
        }

        private void OnLoginSuccess()
        {
            var mainWindow = new MainWindow
            {
                WindowState = WindowState.Maximized
            };

            // (اختياري لكن مفيد) يخليه MainWindow هو النافذة الرئيسية للتطبيق
            Application.Current.MainWindow = mainWindow;

            mainWindow.Show();
            this.Close();
        }


        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
