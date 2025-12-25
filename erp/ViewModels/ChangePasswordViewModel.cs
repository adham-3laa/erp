using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace erp.ViewModels
{
    public class ChangePasswordViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly string _userId;

        public Action? CloseAction { get; set; }

        // ⬅️ لاحظ: بنستقبل الـ session من الخارج
        public ChangePasswordViewModel(string userId, IAuthSession authSession)
        {
            _userId = userId;

            var api = new ApiClient(
                ApiClient.CreateHttpClient(),
                authSession
            );

            _userService = new UserService(api);

            ChangePasswordCommand = new AsyncRelayCommand(ChangePasswordAsync);
            CancelCommand = new RelayCommand(() => CloseAction?.Invoke());
        }

        // ================= PROPERTIES =================

        private string _newPassword;
        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _hasSuccess;
        public bool HasSuccess
        {
            get => _hasSuccess;
            set => SetProperty(ref _hasSuccess, value);
        }

        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        // ================= COMMANDS =================

        public ICommand ChangePasswordCommand { get; }
        public ICommand CancelCommand { get; }

        // ================= LOGIC =================

        private async Task ChangePasswordAsync()
        {
            HasError = false;
            HasSuccess = false;

            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                HasError = true;
                ErrorMessage = "كلمة المرور الجديدة مطلوبة";
                return;
            }

            try
            {
                IsLoading = true;

                var response = await _userService.ChangePasswordAsync(
                    _userId,
                    NewPassword
                );

                if (response.StatusCode == 200)
                {
                    HasSuccess = true;
                    SuccessMessage = "تم تغيير كلمة المرور بنجاح";
                }
                else
                {
                    HasError = true;
                    ErrorMessage = response.Message;
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}
