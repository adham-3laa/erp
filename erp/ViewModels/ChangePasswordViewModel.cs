using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.Services;
using erp.Views.Users;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels
{
    public class ChangePasswordViewModel : ObservableObject
    {
        private readonly UserService _userService;
        private readonly string _userId;

        private string _newPassword;
        private string _confirmPassword;
        private bool _isLoading;
        private string _errorMessage;
        private string _successMessage;

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                ChangePasswordCommand.NotifyCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                SetProperty(ref _errorMessage, value);
                OnPropertyChanged(nameof(HasError));
            }
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                SetProperty(ref _successMessage, value);
                OnPropertyChanged(nameof(HasSuccess));
            }
        }

        public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
        public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);

        public RelayCommand ChangePasswordCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ChangePasswordViewModel(string userId)
        {
            _userId = userId;
            _userService = new UserService(App.Api);

            ChangePasswordCommand = new RelayCommand(
                async () => await ChangePasswordAsync(),
                () => !IsLoading);

            CancelCommand = new RelayCommand(CloseWindow);
        }

        private async Task ChangePasswordAsync()
        {
            ErrorMessage = string.Empty;
            SuccessMessage = string.Empty;

            if (!ValidateInput())
                return;

            try
            {
                IsLoading = true;

                var response = await _userService.ChangePasswordAsync(
                    _userId,
                    NewPassword
                );

                if (response != null && response.StatusCode == 200)
                {
                    SuccessMessage = "تم تغيير كلمة المرور بنجاح";
                    NewPassword = string.Empty;
                    ConfirmPassword = string.Empty;

                    await Task.Delay(1200);
                    CloseWindow();
                }
                else
                {
                    ErrorMessage = response?.Message ?? "فشل تغيير كلمة المرور";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                Debug.WriteLine(ex);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(NewPassword))
            {
                ErrorMessage = "ادخل كلمة المرور الجديدة";
                return false;
            }

            if (NewPassword.Length < 6)
            {
                ErrorMessage = "كلمة المرور لازم 6 حروف على الأقل";
                return false;
            }

            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "كلمتين المرور مش متطابقين";
                return false;
            }

            return true;
        }

        private void CloseWindow()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var window = Application.Current.Windows
                    .OfType<ChangePasswordWindow>()
                    .FirstOrDefault(w => w.DataContext == this);

                window?.Close();
            });
        }
    }
}
