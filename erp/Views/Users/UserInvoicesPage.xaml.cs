using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using erp.DTOS;
using erp.ViewModels;

namespace erp.Views.Users
{
    /// <summary>
    /// Interaction logic for UserInvoicesPage.xaml
    /// </summary>
    public partial class UserInvoicesPage : UserControl
    {
        public UserInvoicesPage(UserDto user)
        {
            InitializeComponent();
            DataContext = new UserInvoicesViewModel(user);
        }
    }

}
