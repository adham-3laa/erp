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

        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        public bool HasUserData => CurrentUser != null;

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

                var parts = CurrentUser.Fullname
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

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
        public ICommand RefreshCommand { get; }

        public CurrentUserViewModel()
        {
            _userService = new UserService(App.Api);

            BackCommand = new RelayCommand(OnBack);
            EditProfileCommand = new RelayCommand(OnEditProfile);
            ChangePasswordCommand = new RelayCommand(OnChangePassword);
            RefreshCommand = new RelayCommand(async () => await LoadCurrentUserAsync());

            // لا تحمل البيانات هنا
        }

        private void OnBack()
        {
            Debug.WriteLine("[CurrentUser] Back button clicked");
            // استخدم الطريقة الصحيحة للرجوع
            // NavigationService.NavigateToUsers();

            // بديل مؤقت حتى تحدد طريقة التنقل الصحيحة:
            Application.Current.MainWindow?.Focus();
        }

        private void OnEditProfile()
        {
            Debug.WriteLine("[CurrentUser] Edit profile button clicked");
            MessageBox.Show("صفحة تعديل الملف الشخصي قيد التطوير", "قريباً",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnChangePassword()
        {
            Debug.WriteLine("[CurrentUser] Change password button clicked");

            try
            {
                // التحقق من وجود بيانات المستخدم
                if (CurrentUser == null || string.IsNullOrEmpty(CurrentUser.Id))
                {
                    ErrorMessage = "تعذر تحميل بيانات المستخدم. الرجاء المحاولة مرة أخرى.";
                    return;
                }

                // فتح نافذة تغيير كلمة المرور
                var changePasswordWindow = new ChangePasswordWindow(CurrentUser.Id)
                {
                    Owner = Application.Current.MainWindow,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                };

                var result = changePasswordWindow.ShowDialog();

                if (result == true)
                {
                    // تم تغيير كلمة المرور بنجاح
                    Debug.WriteLine("[CurrentUser] تم تغيير كلمة المرور بنجاح");

                    // يمكنك إضافة رسالة نجاح هنا إذا أردت
                    // SuccessMessage = "تم تغيير كلمة المرور بنجاح!";
                }
                else if (result == false)
                {
                    Debug.WriteLine("[CurrentUser] ألغى المستخدم العملية");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CurrentUser] خطأ في فتح نافذة تغيير كلمة المرور: {ex.Message}");
                ErrorMessage = $"حدث خطأ: {ex.Message}";
            }
        }
        // إضافة خاصية جديدة
        private string _successMessage;
        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }
        public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

        public async Task LoadCurrentUserAsync()
        {
            try
            {
                // إعادة تعيين حالة الخطأ
                HasError = false;
                ErrorMessage = string.Empty;

                // بدء التحميل
                IsLoading = true;
                Debug.WriteLine($"[CurrentUser] [{DateTime.Now:HH:mm:ss}] بدء تحميل بيانات المستخدم...");

                var response = await _userService.GetCurrentUserAsync();

                if (response != null)
                {
                    Debug.WriteLine($"[CurrentUser] [{DateTime.Now:HH:mm:ss}] تم استلام الاستجابة:");
                    Debug.WriteLine($"  - الاسم: {response.Fullname}");
                    Debug.WriteLine($"  - البريد: {response.Email}");
                    Debug.WriteLine($"  - النوع: {response.UserType}");
                    Debug.WriteLine($"  - الحالة: {(response.IsActive ? "نشط" : "غير نشط")}");
                    Debug.WriteLine($"  - عدد المزارع: {response.FarmsCount}");

                    CurrentUser = response;
                    Debug.WriteLine($"[CurrentUser] [{DateTime.Now:HH:mm:ss}] تم تعيين بيانات المستخدم في ViewModel");
                }
                else
                {
                    Debug.WriteLine($"[CurrentUser] [{DateTime.Now:HH:mm:ss}] فشل: الاستجابة فارغة");

                    HasError = true;
                    ErrorMessage = "لم يتم العثور على بيانات المستخدم. الرجاء المحاولة مرة أخرى.";

                    MessageBox.Show("تعذر تحميل بيانات المستخدم. يرجى التأكد من الاتصال بالإنترنت والمحاولة مرة أخرى.",
                        "خطأ في التحميل", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CurrentUser] [{DateTime.Now:HH:mm:ss}] حدث خطأ:");
                Debug.WriteLine($"  - الرسالة: {ex.Message}");
                Debug.WriteLine($"  - Stack Trace: {ex.StackTrace}");

                HasError = true;
                ErrorMessage = $"حدث خطأ: {ex.Message}";

                Debug.WriteLine($"[CurrentUser] عرض رسالة الخطأ للمستخدم");
                MessageBox.Show($"حدث خطأ أثناء تحميل بيانات المستخدم:\n{ex.Message}",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
                Debug.WriteLine($"[CurrentUser] [{DateTime.Now:HH:mm:ss}] انتهى التحميل. IsLoading = false");
            }
        }

    }
}