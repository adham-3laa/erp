using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using erp.Commands;
using erp.DTOs;
using erp.Services;

namespace erp.ViewModels.Auth;

public sealed class LoginViewModel : BaseViewModel
{
    private readonly AuthService _auth;
    private CancellationTokenSource? _cts;

    public LoginViewModel(AuthService auth)
    {
        _auth = auth;

        LoginCommand = new AsyncRelayCommand(LoginAsync, CanLogin);
        LogoutCommand = new AsyncRelayCommand(LogoutAsync, () => !IsBusy);
    }

    private string _email = "";
    public string Email
    {
        get => _email;
        set
        {
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
            {
                LoginCommand.RaiseCanExecuteChanged();
                LogoutCommand.RaiseCanExecuteChanged();
            }
        }
    }

    private string? _message;
    public string? Message { get => _message; private set => Set(ref _message, value); }

    public AsyncRelayCommand LoginCommand { get; }
    public AsyncRelayCommand LogoutCommand { get; }

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
                new LoginRequest { Email = Email.Trim(), Password = Password },
                _cts.Token);

            // ✅ نجاح فعلي (200 + token)
            if (status == HttpStatusCode.OK &&
                result?.Success == true &&
                !string.IsNullOrWhiteSpace(result.Auth?.Token))
            {
                Message = "✅ Login Successful";
                return;
            }

            // ✅ 202 (OTP/Confirm) - انت مش هتدعمه دلوقتي
            if (status == HttpStatusCode.Accepted)
            {
                Message = $"⚠ {result?.Message ?? "Login needs OTP/confirmation."} (Not supported الآن)";
                return;
            }

            // باقي الحالات
            Message = $"❌ {result?.Message ?? "Login failed."} ({result?.ErrorCode ?? "N/A"})";
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LogoutAsync()
    {
        try
        {
            IsBusy = true;
            var res = await _auth.LogoutAsync();
            Message = res?.Message ?? "Logged out.";
        }
        catch (Exception ex)
        {
            Message = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
