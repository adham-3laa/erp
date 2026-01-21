using erp.Commands;
using erp.DTOS.Auth.Requests;
using erp.Services;
using erp.ViewModels;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels.Auth
{
    public sealed class LoginViewModel : BaseViewModel
    {
        private readonly AuthService _auth;
        private readonly Action _onLoginSuccess;
        private CancellationTokenSource? _cts;

        // ✅ Timer لرسالة الـ UI
        private CancellationTokenSource? _messageCts;

        public LoginViewModel(AuthService auth, Action onLoginSuccess)
        {
            _auth = auth;
            _onLoginSuccess = onLoginSuccess;
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
        }

        private string _email = "admin@gmail.com";
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                    LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                    LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (SetProperty(ref _isBusy, value))
                    LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string? _message;
        public string? Message
        {
            get => _message;
            private set => SetProperty(ref _message, value);
        }

        public AsyncRelayCommand LoginCommand { get; }

        private bool CanLogin() =>
            !IsBusy &&
            !string.IsNullOrWhiteSpace(Email) &&
            !string.IsNullOrWhiteSpace(Password);

        private async Task LoginAsync()
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            SetMessageAutoHide(null);

            try
            {
                IsBusy = true;

                var (status, result) = await _auth.LoginAsync(
                    new LoginRequest(Email.Trim(), Password),
                    _cts.Token);

                // ✅ تسجيل دخول ناجح
                if (status == HttpStatusCode.OK &&
                    result?.Success == true &&
                    !string.IsNullOrWhiteSpace(result.Auth?.Token))
                {
                    TokenStore.Token = result.Auth.Token;
                    _onLoginSuccess.Invoke();
                    return;
                }

                // ❌ بيانات غير صحيحة (رسالة من السيرفر)
                if (result != null && result.Success == false)
                {
                    var clean = SanitizeApiMessage(result.Message);
                    SetMessageAutoHide($"❌ {clean}");
                    return;
                }

                // ❌ حالة غير متوقعة
                SetMessageAutoHide("❌ فشل تسجيل الدخول. تحقق من بياناتك");
            }
            catch (Exception ex)
            {
                SetMessageAutoHide($"❌ {SanitizeApiMessage(ex.Message)}");
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ✅ تعرض الرسالة ثم تخفيها بعد 5 ثواني
        private void SetMessageAutoHide(string? value)
        {
            // اعرض الرسالة فورًا
            Message = value;

            // ألغِ أي عدّاد قديم
            _messageCts?.Cancel();
            _messageCts = null;

            // لو مفيش رسالة، خلاص
            if (string.IsNullOrWhiteSpace(value))
                return;

            var current = value;
            _messageCts = new CancellationTokenSource();
            var token = _messageCts.Token;

            _ = HideMessageAfterDelayAsync(current, token);
        }

        private async Task HideMessageAfterDelayAsync(string currentMessage, CancellationToken token)
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(3), token);

                if (token.IsCancellationRequested)
                    return;

                // امسحها على UI thread وبشرط إنها لسه نفس الرسالة
                Application.Current?.Dispatcher?.Invoke(() =>
                {
                    if (Message == currentMessage)
                        Message = null;
                });
            }
            catch (TaskCanceledException)
            {
                // ignore
            }
        }

        // ✅ تنضيف رسائل السيرفر اللي فيها traceId / ProblemDetails
        private static string SanitizeApiMessage(string? msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return "البريد الإلكتروني أو كلمة المرور خاطئة";

            if (msg.Contains("traceId", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("errors", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("{", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("validation", StringComparison.OrdinalIgnoreCase))
            {
                return "البريد الإلكتروني أو كلمة المرور خاطئة";
            }

            return msg.Trim();
        }
    }
}
