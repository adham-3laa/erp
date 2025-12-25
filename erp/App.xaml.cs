using System.Net.Http;
using System.Windows;
using erp.Services;
using erp.Services.Category;
using erp.Views.Auth;

namespace erp
{
    public partial class App : Application
    {
        public static HttpClient Http { get; private set; } = null!;
        public static ApiClient Api { get; private set; } = null!;
        public static CategoryService Categories { get; private set; } = null!;

        public static DashboardService Dashboard { get; private set; } = null!;

        // Auth
        public static IAuthSession Session { get; private set; } = null!;
        public static AuthService Auth { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Session = new AuthSession();

            Http = ApiClient.CreateHttpClient("http://be-positive.runasp.net/");
            Api = new ApiClient(Http, Session);

            Categories = new CategoryService(Api);
            Auth = new AuthService(Api, Session);

            // ✅ يعتمد على ApiClient عشان Bearer Token
            Dashboard = new DashboardService(Api);

            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}
