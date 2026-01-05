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
    new UserTypeOption("مدير النظام", "SystemAdmin"),
    new UserTypeOption("مستخدم", "User"),
    new UserTypeOption("عميل", "Customer"),
    new UserTypeOption("مندوب مبيعات", "SalesRep"),
    new UserTypeOption("مدير مخزن", "StoreManager"),
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
            if (string.IsNullOrWhiteSpace(Fullname))
                return Fail("الاسم الكامل مطلوب");

            if (string.IsNullOrWhiteSpace(Email))
                return Fail("البريد الإلكتروني مطلوب");
          
            if (SelectedUserType == null)
                return Fail("من فضلك اختر نوع المستخدم");



            if (string.IsNullOrWhiteSpace(Password))
                return Fail("كلمة المرور مطلوبة");

            // تحقق من طول كلمة المرور
            if (Password.Length < 6)
                return Fail("كلمة المرور يجب أن تكون أكبر من 6 حروف");

            if (Password != ConfirmPassword)
                return Fail("كلمتا المرور غير متطابقتين");

            return true;
        }
        private async Task CreateUserAsync()
        {
            HasError = false;

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

                if (result != null && result.StatusCode == 200)
                {
                    MessageBox.Show(
                        "تم إنشاء المستخدم بنجاح",
                        "نجاح",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    ClearForm();
                }
                else
                {
                    // إذا كانت الرسالة تحتوي على "البريد الإلكتروني مكرر"
                    if (result?.Message.Contains("البريد الإلكتروني") == true || result?.Message.Contains("اسم المستخدم") == true)
                    {
                        HasError = true;
                        ErrorMessage = "البريد الإلكتروني أو اسم المستخدم مكرر. يرجى التحقق وإعادة المحاولة.";
                    }
                    else if (result?.Message.Contains("An error occurred while saving the entity changes") == true)
                    {
                        HasError = true;
                        ErrorMessage = "لبريد الإلكتروني أو اسم المستخدم مكرر. يرجى التحقق وإعادة المحاولة.";
                    }
                    else
                    {
                        HasError = true;
                        ErrorMessage = result?.Message ?? "حدث خطأ أثناء إنشاء المستخدم.";
                    }
                }
            }
            catch (Exception ex)
            {
                HasError = true;
                ErrorMessage = "لبريد الإلكتروني أو اسم المستخدم مكرر. يرجى التحقق وإعادة المحاولة ";
            }
            finally
            {
                IsLoading = false;
            }
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
        }
    }

}
