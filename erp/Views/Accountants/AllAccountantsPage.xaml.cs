using System.Windows.Controls;

namespace EduGate.Views.Accountants
{
    public partial class AllAccountantsPage : Page
    {
        public AllAccountantsViewModel ViewModel { get; set; }

        public AllAccountantsPage()
        {
            InitializeComponent();
            ViewModel = new AllAccountantsViewModel(); // هنا تربط الـ ViewModel
            DataContext = ViewModel;
        }
    }
}
