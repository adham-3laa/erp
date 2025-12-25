using erp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace erp.Views.Users
{
    public partial class CurrentUserPage : Page
    {
        private readonly CurrentUserViewModel _viewModel;

        public CurrentUserPage()
        {
            InitializeComponent();
            _viewModel = new CurrentUserViewModel();
            DataContext = _viewModel;
            Loaded += Page_Loaded;
            Unloaded += Page_Unloaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // تحميل البيانات مرة واحدة فقط
            await _viewModel.LoadCurrentUserAsync();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            // تنظيف الأحداث لمنع تسرب الذاكرة
            Loaded -= Page_Loaded;
            Unloaded -= Page_Unloaded;
        }
    }
}