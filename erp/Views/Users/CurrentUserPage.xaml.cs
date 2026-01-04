using erp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Threading.Tasks;

namespace erp.Views.Users
{
    public partial class CurrentUserPage : Page
    {
        private readonly CurrentUserViewModel _viewModel;
        private readonly string? _userId;

        public CurrentUserPage(string? userId = null)
        {
            InitializeComponent();
            _userId = userId;

            _viewModel = new CurrentUserViewModel(_userId);
            DataContext = _viewModel;

            Loaded += Page_Loaded;
            Unloaded += Page_Unloaded;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            await _viewModel.LoadUserAsync(); // تحميل بالـ id لو موجود
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Loaded -= Page_Loaded;
            Unloaded -= Page_Unloaded;
        }
    }

}