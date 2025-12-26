using erp.Commands;
using erp.DTOS.Auth.Requests;
using erp.Services;
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

        public LoginViewModel(AuthService auth, Action onLoginSuccess)
        {
            _auth = auth;
            _onLoginSuccess = onLoginSuccess;
            LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
        }

        private string _email = "";
        public string Email
        {
            get => _email;
            set
            {
                // ✅ حفظ الـ JWT Token في TokenStore
                TokenStore.Token = result.Auth.Token;

                _onLoginSuccess.Invoke();
                return;

                if (Set(ref _email, value))
                    LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string _password = "";
        public string Password
        {
            get => _password;
            set
            {
                if (Set(ref _password, value))
                    LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                if (Set(ref _isBusy, value))
                    LoginCommand.RaiseCanExecuteChanged();
            }
        }

        private string? _message;
        public string? Message
        {
            get => _message;
            private set => Set(ref _message, value);
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
            Message = null;

            try
            {
                IsBusy = true;

                var (status, result) = await _auth.LoginAsync(
                    new LoginRequest(Email.Trim(), Password),
                    _cts.Token);

                // ✅ خزّن التوكن بعد تسجيل الدخول الناجح
                if (status == HttpStatusCode.OK && result?.Success == true && !string.IsNullOrWhiteSpace(result.Auth?.Token))
                {
                    TokenStore.Token = result.Auth.Token; // <-- تخزين التوكن
                    _onLoginSuccess.Invoke();
                    return;
                }

                // لو success = false أو أي خطأ في البيانات
                if (result != null && result.Success == false)
                {
                    Message = $"❌ {result.Message ?? "البريد الإلكتروني أو كلمة المرور خاطئة"}";
                    return;
                }

                // أي حالة ثانية
                Message = "❌ فشل تسجيل الدخول. تحقق من بياناتك";
            }
            catch (Exception ex)
            {
                Message = $"❌ حدث خطأ: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

}


