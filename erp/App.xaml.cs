using System.Net.Http;
using System.Windows;
using erp.Services;

namespace erp
{
    public partial class App : Application
    {
        public static HttpClient Http { get; private set; } = null!;
        public static ApiClient Api { get; private set; } = null!;
        public static CategoryService Categories { get; private set; } = null!;
        public static AccountantService Accountants { get; private set; } = null!;

        // ✅ NEW (Auth)
        public static IAuthSession Session { get; private set; } = null!;
        public static AuthService Auth { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Session = new AuthSession();

            Http = ApiClient.CreateHttpClient(Session.AccessToken);
            Api = new ApiClient(Http);

            Categories = new CategoryService(Api);
            Accountants = new AccountantService(Http);

            Auth = new AuthService(Api, Session);
        }

    }
}
