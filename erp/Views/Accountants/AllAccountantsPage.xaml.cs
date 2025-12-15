using System.Windows.Controls;
using erp.ViewModels;

namespace EduGate.Views.Accountants
{
    public partial class AllAccountantsPage : Page
    {
        public AllAccountantsViewModel ViewModel { get; }

        public AllAccountantsPage()
        {
            InitializeComponent();
            ViewModel = new AllAccountantsViewModel();
            DataContext = ViewModel;

            Loaded += async (_, __) => await ViewModel.LoadCommand.ExecuteAsync(null);
        }
    }
}
