using System.Windows.Controls;

namespace EduGate.Views.Accountants
{
    public partial class CurrentAccountantPage : Page
    {
        public CurrentAccountantViewModel ViewModel { get; set; }

        public CurrentAccountantPage()
        {
            InitializeComponent();
            ViewModel = new CurrentAccountantViewModel();
            DataContext = ViewModel;
        }
    }
}
