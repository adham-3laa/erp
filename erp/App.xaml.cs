using System.Net.Http;
using System.Windows;
using erp.Services;
using erp.Services.Category;
using erp.Views.Auth;

// ✅ QuestPDF
using QuestPDF.Infrastructure;

namespace erp
{
    public partial class App : Application
    {
        public static IAuthSession AuthSession { get; } = new AuthSession();
        public static HttpClient Http { get; private set; } = null!;
        public static ApiClient Api { get; private set; } = null!;
        public static CategoryService Categories { get; private set; } = null!;
        public static UserService Users { get; private set; } = null!; // ✅ إضافة

        public static DashboardService Dashboard { get; private set; } = null!;

        // Auth
        public static IAuthSession Session { get; private set; }
        public static AuthService Auth { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ✅ QuestPDF License (مهم جدًا – سطر واحد بس)
            QuestPDF.Settings.License = LicenseType.Community;

            Session = new AuthSession();

            Http = ApiClient.CreateHttpClient("http://be-positive.runasp.net/");
            Api = new ApiClient(Http, Session);

            Categories = new CategoryService(Api);
            Users = new UserService(Api);              // ✅ تهيئة
            Auth = new AuthService(Api, Session);

            // ✅ يعتمد على ApiClient عشان Bearer Token
            Dashboard = new DashboardService(Api);

            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}
