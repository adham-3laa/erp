using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using erp.Views.Users;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace erp.ViewModels
{
    public class CurrentUserViewModel : ObservableObject
    {
        private readonly UserService _userService;

        private CurrentUserDto _currentUser;
        private bool _isLoading;

        private string _errorMessage;
        private bool _hasError;

        private string _successMessage;
        private readonly string? _userId;


        public CurrentUserDto CurrentUser
        {
            get => _currentUser;
            set
            {
                if (SetProperty(ref _currentUser, value))
                {
                    OnPropertyChanged(nameof(FormattedDate));
                    OnPropertyChanged(nameof(UserStatusText));
                    OnPropertyChanged(nameof(UserStatusColor));
                    OnPropertyChanged(nameof(Initials));
                    OnPropertyChanged(nameof(HasUserData));
                }
            }
        }

        public bool HasUserData => CurrentUser != null;

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (SetProperty(ref _errorMessage, value))
                    OnPropertyChanged(nameof(HasError));
            }
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set
            {
                if (SetProperty(ref _successMessage, value))
                    OnPropertyChanged(nameof(HasSuccess));
            }
        }

        public bool HasSuccess => !string.IsNullOrWhiteSpace(SuccessMessage);

        public string FormattedDate
        {
            get
            {
                if (CurrentUser == null || CurrentUser.DateOfCreation.Year <= 1)
                    return "غير محدد";

                return CurrentUser.DateOfCreation.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            }
        }

        public string UserStatusText => CurrentUser?.IsActive == true ? "نشط" : "غير نشط";
        public string UserStatusColor => CurrentUser?.IsActive == true ? "#10B981" : "#EF4444";

        public string Initials
        {
            get
            {
                if (CurrentUser == null || string.IsNullOrWhiteSpace(CurrentUser.Fullname))
                    return "??";

                var parts = CurrentUser.Fullname.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[1][0]}".ToUpper();

                return parts[0].Length >= 2
                    ? $"{parts[0][0]}{parts[0][1]}".ToUpper()
                    : parts[0][0].ToString().ToUpper();
            }
        }

        public ICommand BackCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        // ✅ صح للـ async
        public IAsyncRelayCommand RefreshCommand { get; }

        public CurrentUserViewModel(string? userId = null)
        {
            _userId = userId;
            _userService = new UserService(App.Api);

            BackCommand = new RelayCommand(OnBack);
            EditProfileCommand = new RelayCommand(OnEditProfile);
            ChangePasswordCommand = new RelayCommand(OnChangePassword);
            RefreshCommand = new AsyncRelayCommand(LoadUserAsync);
        }


        private void OnBack()
        {
            Debug.WriteLine("[CurrentUser] Back button clicked");
            Application.Current.MainWindow?.Focus();
        }

        private void OnEditProfile()
        {
            Debug.WriteLine("[CurrentUser] Edit profile button clicked");

            if (CurrentUser == null || string.IsNullOrWhiteSpace(CurrentUser.Id))
            {
                SetError("تعذر تحميل بيانات المستخدم");
                return;
            }

            NavigationService.NavigateToUpdateUser(CurrentUser.Id);
        }

        private void OnChangePassword()
        {
            Debug.WriteLine("[CurrentUser] Change password button clicked");

            try
            {
                ClearMessages();

                if (CurrentUser == null || string.IsNullOrWhiteSpace(CurrentUser.Id))
                {
                    SetError("تعذر تحميل بيانات المستخدم. الرجاء المحاولة مرة أخرى.");
                    return;
                }

                var changePasswordWindow = new ChangePasswordWindow(
                    CurrentUser.Id,
                    App.Session
                )
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                // ✅ مرة واحدة بس
                var result = changePasswordWindow.ShowDialog();

                if (result == true)
                {
                    Debug.WriteLine("[CurrentUser] تم تغيير كلمة المرور بنجاح");
                    SuccessMessage = "تم تغيير كلمة المرور بنجاح ✅";
                }
                else
                {
                    Debug.WriteLine("[CurrentUser] المستخدم أغلق النافذة أو ألغى العملية");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CurrentUser] خطأ في فتح نافذة تغيير كلمة المرور: {ex.Message}");
                SetError($"حدث خطأ: {ex.Message}");
            }
        }

        public async Task LoadUserAsync()
        {
            try
            {
                HasError = false;
                ErrorMessage = string.Empty;
                IsLoading = true;

                CurrentUserDto? response;

                // ✅ لو الصفحة اتفتحت بـ id → هات المستخدم ده
                if (!string.IsNullOrWhiteSpace(_userId))
                    response = await _userService.GetUserDetailsByIdAsync(_userId);
                else
                    response = await _userService.GetCurrentUserAsync();

                if (response != null)
                {
                    CurrentUser = response;
                }
                else
                {
                    HasError = true;
                    ErrorMessage = "لم يتم العثور على بيانات المستخدم.";
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = $"حدث خطأ: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }


        // ===================== Helpers =====================
        private void ClearMessages()
        {
            HasError = false;
            ErrorMessage = string.Empty;

            SuccessMessage = string.Empty;
        }

        private void SetError(string message)
        {
            HasError = true;
            ErrorMessage = message;
        }
    }
}
