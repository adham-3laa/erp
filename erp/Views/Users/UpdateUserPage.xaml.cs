using System.Windows.Controls;

namespace erp.Views.Users
{
    public partial class UpdateUserPage : Page
    {
        public UpdateUserPage(string userId)
        {
            InitializeComponent();
            DataContext = new ViewModels.UpdateUserViewModel(userId);
        }
    }
}