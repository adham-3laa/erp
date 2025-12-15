using System.Windows.Controls;
using erp.ViewModels;

namespace EduGate.Views.Accountants
{
    public partial class AddAccountantPage : Page
    {
        public AddAccountantViewModel ViewModel { get; }

        public AddAccountantPage()
        {
            InitializeComponent();
            ViewModel = new AddAccountantViewModel();
            DataContext = ViewModel;

            // PasswordBox ما بيدعمش Binding مباشر، فده حل بسيط وواضح
            PasswordBox.PasswordChanged += (_, __) => ViewModel.Password = PasswordBox.Password;
        }
    }
}
