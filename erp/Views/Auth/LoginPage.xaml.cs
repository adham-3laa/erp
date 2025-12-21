using System.Windows.Controls;
using erp.ViewModels.Auth;

namespace erp.Views.Auth;

public partial class LoginPage : Page
{
    public LoginPage()
    {
        InitializeComponent();
        DataContext = new LoginViewModel(App.Auth);
    }

    private void Login_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        // PasswordBox مش بيعمل Binding مباشر
        if (DataContext is LoginViewModel vm)
            vm.Password = Pwd.Password;
    }
}
