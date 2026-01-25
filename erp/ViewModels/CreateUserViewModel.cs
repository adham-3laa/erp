using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace erp.ViewModels
{
    public class CreateUserViewModel : ObservableObject
    {
        private readonly UserService _userService;

        public CreateUserViewModel()
        {
            _userService = App.Users;

            UserTypes = new ObservableCollection<UserTypeOption>
{
    // new UserTypeOption("مدير النظام", "SystemAdmin"),

    //new UserTypeOption("عميل", "Customer"),
    //new UserTypeOption("مندوب مبيعات", "SalesRep"),
    // new UserTypeOption("مدير مخزن", "StoreManager"),
    new UserTypeOption("مورد", "Supplier"),
    new UserTypeOption("محاسب", "Accountant")
};



            CreateCommand = new AsyncRelayCommand(CreateUserAsync);
            CancelCommand = new RelayCommand(OnCancel);
        }

        // ================== PROPERTIES ==================

        public ObservableCollection<UserTypeOption> UserTypes { get; }

        private UserTypeOption _selectedUserType;
        public UserTypeOption SelectedUserType
        {
            get => _selectedUserType;
            set => SetProperty(ref _selectedUserType, value);
        }


        private string _fullname;
        public string Fullname
        {
            get => _fullname;
            set => SetProperty(ref _fullname, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _phoneNumber;
        public string PhoneNumber
        {
            get => _phoneNumber;
            set => SetProperty(ref _phoneNumber, value);
        }

        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }


        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        private bool _hasError;
        public bool HasError
        {
            get => _hasError;
            set => SetProperty(ref _hasError, value);
        }

        // ================== COMMANDS ==================

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        // ================== LOGIC ==================

        private bool Validate()
        {
            if (SelectedUserType == null)
                return Fail("من فضلك اختر نوع المستخدم");

            if (string.IsNullOrWhiteSpace(Fullname))
                return Fail("الاسم الكامل مطلوب");

            // التحقق من أن الاسم ثلاثي (3 أجزاء بالضبط)
            var nameParts = Fullname.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (nameParts.Length != 3)
                return Fail("الاسم يجب أن يكون ثلاثي فقط (لا يقل ولا يزيد عن 3 أسماء)");

            return true;
        }
        private async Task CreateUserAsync()
        {
            HasError = false;
            ErrorMessage = null;

            if (!Validate())
                return;

            try
            {
                IsLoading = true;

                var dto = new UserPostDto
                {
                    Fullname = Fullname,
                    Email = Email,
                    Password = Password,
                    PhoneNumber = PhoneNumber,
                    UserType = SelectedUserType.Value
                };

                var result = await _userService.CreateUserAsync(dto);

                // ✅ نجاح العملية
                if (result != null && result.StatusCode == 200)
                {
                    MessageBox.Show(
                        "تم إنشاء المستخدم بنجاح",
                        "نجاح",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    ClearForm();
                    return;
                }

                // ❌ فشل العملية - معالجة الأخطاء
                var errorMsg = SanitizeErrorMessage(result?.Message);
                HasError = true;
                ErrorMessage = errorMsg;
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = SanitizeErrorMessage(ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ✅ تنظيف رسائل الأخطاء من السيرفر
        private static string SanitizeErrorMessage(string? message)
        {
            if (string.IsNullOrWhiteSpace(message))
                return "حدث خطأ أثناء إنشاء المستخدم. يرجى المحاولة مرة أخرى.";

            var msg = message.Trim();

            // إزالة معلومات التتبع الفنية
            if (msg.Contains("traceId", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("errors", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("{", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("validation", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("An error occurred while saving the entity changes", StringComparison.OrdinalIgnoreCase))
            {
                // التحقق من نوع الخطأ المحدد
                if (msg.Contains("email", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("البريد الإلكتروني", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("username", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("اسم المستخدم", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("duplicate", StringComparison.OrdinalIgnoreCase) ||
                    msg.Contains("مكرر", StringComparison.OrdinalIgnoreCase))
                {
                    return "البريد الإلكتروني أو اسم المستخدم مكرر. يرجى التحقق وإعادة المحاولة.";
                }

                return "حدث خطأ أثناء إنشاء المستخدم. يرجى التحقق من البيانات والمحاولة مرة أخرى.";
            }

            // إرجاع الرسالة الأصلية بعد التنظيف
            return msg;
        }



        private bool Fail(string message)
        {
            HasError = true;
            ErrorMessage = message;
            return false;
        }

        private void ClearForm()
        {
            Fullname = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
        }
        public class UserTypeOption
        {
            public string Text { get; set; }
            public string Value { get; set; }

            public UserTypeOption(string text, string value)
            {
                Text = text;
                Value = value;
            }
        }

        private void OnCancel()
        {
            ClearForm();
            NavigationService.NavigateBack();
        }

    }

}
