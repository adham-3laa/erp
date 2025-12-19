using System.Windows.Controls;

namespace erp.Views.Users
{
    public partial class CurrentUserPage : Page
    {
        public CurrentUserPage()
        {
            InitializeComponent();
            DataContext = new ViewModels.CurrentUserViewModel();
        }
    }
}
