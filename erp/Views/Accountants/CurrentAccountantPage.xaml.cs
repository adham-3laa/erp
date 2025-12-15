using System.Windows.Controls;
using erp.ViewModels;

namespace EduGate.Views.Accountants
{
    public partial class CurrentAccountantPage : Page
    {
        public CurrentAccountantViewModel ViewModel { get; }

        public CurrentAccountantPage()
        {
            InitializeComponent();
            ViewModel = new CurrentAccountantViewModel();
            DataContext = ViewModel;

            Loaded += async (_, __) => await ViewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
