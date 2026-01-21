using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
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
        public static ChequeService Cheques { get; private set; } = null!;

        // Auth
        public static IAuthSession Session { get; private set; }

        public static AuthService Auth { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // ═══════════════════════════════════════════════════════════
            // Global Exception Handlers - CRITICAL for app stability
            // ═══════════════════════════════════════════════════════════
            
            // Handle exceptions on the UI thread
            DispatcherUnhandledException += App_DispatcherUnhandledException;
            
            // Handle exceptions from non-UI threads
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            // Handle exceptions from async Task methods
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;

            // ═══════════════════════════════════════════════════════════
            // Application Initialization
            // ═══════════════════════════════════════════════════════════

            // ✅ QuestPDF License (مهم جدًا – سطر واحد بس)
            QuestPDF.Settings.License = LicenseType.Community;

            Session = new AuthSession();

            Http = ApiClient.CreateHttpClient("http://warhouse.runasp.net/");
            Api = new ApiClient(Http, Session);

            Categories = new CategoryService(Api);
            Users = new UserService(Api);              // ✅ تهيئة
            Auth = new AuthService(Api, Session);

            // ✅ يعتمد على ApiClient عشان Bearer Token
            Dashboard = new DashboardService(Api);
            Cheques = new ChequeService(Api);

            ErrorHandlingService.LogInfo("Application started successfully.");

            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }

        /// <summary>
        /// Handles unhandled exceptions on the UI (Dispatcher) thread.
        /// Shows Arabic error message and prevents app crash.
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Log the full exception for developers
            ErrorHandlingService.LogError(e.Exception, "Unhandled UI Thread Exception");

            // Show user-friendly Arabic message
            var message = ErrorHandlingService.GetUserFriendlyMessage(e.Exception);
            ErrorHandlingService.ShowError(message + "\n\n" + ErrorHandlingService.Messages.ContactSupport);

            // Mark as handled to prevent app crash
            e.Handled = true;
        }

        /// <summary>
        /// Handles unhandled exceptions from non-UI threads and the AppDomain.
        /// This is a last-resort handler for catastrophic errors.
        /// </summary>
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception ?? new Exception("Unknown fatal error");
            
            // Log the full exception
            ErrorHandlingService.LogError(exception, "Unhandled AppDomain Exception (Fatal: " + e.IsTerminating + ")");

            // Try to show error message on UI thread
            try
            {
                Dispatcher.Invoke(() =>
                {
                    var message = ErrorHandlingService.GetUserFriendlyMessage(exception);
                    
                    if (e.IsTerminating)
                    {
                        ErrorHandlingService.ShowError(
                            "حدث خطأ فادح في التطبيق.\n\n" +
                            message + "\n\n" +
                            "سيتم إغلاق التطبيق. يرجى إعادة تشغيله والمحاولة مرة أخرى.",
                            "خطأ فادح");
                    }
                    else
                    {
                        ErrorHandlingService.ShowError(message);
                    }
                });
            }
            catch
            {
                // Last resort - native MessageBox
                MessageBox.Show(
                    "حدث خطأ غير متوقع. يرجى إعادة تشغيل التطبيق.",
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Handles exceptions from async Task methods that were not awaited or observed.
        /// Prevents these from crashing the application.
        /// </summary>
        private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            // Log all inner exceptions
            foreach (var ex in e.Exception.InnerExceptions)
            {
                ErrorHandlingService.LogError(ex, "Unobserved Task Exception");
            }

            // Mark as observed to prevent app crash
            e.SetObserved();

            // Show error message on UI thread
            Dispatcher.BeginInvoke(new Action(() =>
            {
                var firstException = e.Exception.InnerExceptions.Count > 0 
                    ? e.Exception.InnerExceptions[0] 
                    : e.Exception;
                    
                var message = ErrorHandlingService.GetUserFriendlyMessage(firstException);
                ErrorHandlingService.ShowError(message);
            }));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ErrorHandlingService.LogInfo("Application exiting.");
            base.OnExit(e);
        }
    }
}

