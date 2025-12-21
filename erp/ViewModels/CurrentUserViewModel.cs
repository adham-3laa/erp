using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using erp.DTOS;
using erp.Services;
using System.Globalization;

namespace erp.ViewModels
{
    public class CurrentUserViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private UserDto _currentUser;

        public UserDto CurrentUser
        {
            get => _currentUser;
            set
            {
                _currentUser = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(FormattedDate));
                OnPropertyChanged(nameof(UserStatusText));
                OnPropertyChanged(nameof(UserStatusColor));
                OnPropertyChanged(nameof(Initials));
            }
        }

        public string FormattedDate => CurrentUser?.DateOfCreation.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        public string UserStatusText => CurrentUser?.IsActive == true ? "نشط" : "غير نشط";
        public string UserStatusColor => CurrentUser?.IsActive == true ? "#10B981" : "#EF4444";

        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(CurrentUser?.Fullname))
                    return "??";

                var parts = CurrentUser.Fullname.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[1][0]}".ToUpper();
                else if (parts.Length == 1 && parts[0].Length >= 2)
                    return $"{parts[0][0]}{parts[0][1]}".ToUpper();

                return "??";
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadCurrentUserCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand EditProfileCommand { get; }
        public ICommand ChangePasswordCommand { get; }

        public CurrentUserViewModel()
        {
            _userService = new UserService();

            LoadCurrentUserCommand = new RelayCommand(async () => await LoadCurrentUserAsync());

            // استخدم NavigationService بدلاً من MainWindow مباشرة
            BackCommand = new RelayCommand(() => erp.Services.NavigationService.NavigateToUsers());
            EditProfileCommand = new RelayCommand(() => ExecuteEditProfile());
            ChangePasswordCommand = new RelayCommand(() => ExecuteChangePassword());

            LoadCurrentUserCommand.Execute(null);
        }

        private async Task LoadCurrentUserAsync()
        {
            try
            {
                IsLoading = true;

                CurrentUser = await _userService.GetCurrentUserAsync();
                if (CurrentUser == null)
                {
                    MessageBox.Show("فشل تحميل بيانات المستخدم");
                }


               
            }
            catch (Exception ex)
            {
                MessageBox.Show($"حدث خطأ: {ex.Message}", "خطأ",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void ExecuteEditProfile()
        {
            MessageBox.Show("صفحة تعديل الملف الشخصي قيد التطوير", "تطوير",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteChangePassword()
        {
            MessageBox.Show("صفحة تغيير كلمة المرور قيد التطوير", "تطوير",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
