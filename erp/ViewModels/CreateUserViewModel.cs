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

            UserTypes = new ObservableCollection<string>
{
    "SystemAdmin",
    "User",
    "Customer",
    "SalesRep",
    "StoreManager",
    "Supplier",
    "Accountant"
};


            SelectedUserType = "SystemAdmin";

            CreateCommand = new AsyncRelayCommand(CreateUserAsync);
            CancelCommand = new RelayCommand(OnCancel);
        }

        // ================== PROPERTIES ==================

        public ObservableCollection<string> UserTypes { get; }

        private string _selectedUserType;
        public string SelectedUserType
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

        private bool _isLoading;
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
                    UserType = SelectedUserType
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
                    HasError = true;
                    ErrorMessage = result?.Message ?? "حدث خطأ أثناء إنشاء المستخدم";
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

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(Fullname))
                return Fail("الاسم الكامل مطلوب");

            if (string.IsNullOrWhiteSpace(Email))
                return Fail("البريد الإلكتروني مطلوب");

            if (string.IsNullOrWhiteSpace(Password))
                return Fail("كلمة المرور مطلوبة");

            if (Password != ConfirmPassword)
                return Fail("كلمتا المرور غير متطابقتين");

            return true;
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

        private void OnCancel()
        {
            ClearForm();
        }
    }
}
