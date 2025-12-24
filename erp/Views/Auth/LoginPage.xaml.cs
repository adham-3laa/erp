using System.Windows;
using System.Windows.Controls;
using erp.ViewModels.Auth;

namespace erp.Views.Auth;

public partial class LoginPage : Page
{
    private readonly Window _parentWindow;

    public LoginPage(Window parentWindow)
    {
        InitializeComponent();
        _parentWindow = parentWindow;

        // ربط الـ ViewModel مع Action عند نجاح تسجيل الدخول
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
        // فتح MainWindow وإغلاق LoginWindow
        var mainWindow = new MainWindow();
        mainWindow.Show();
        _parentWindow.Close();
    }
}
