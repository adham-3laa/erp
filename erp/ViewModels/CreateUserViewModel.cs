using erp.DTOS;
using erp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace erp.ViewModels
{
    public class CreateUserViewModel : BaseViewModel
    {
        private readonly UserService _userService = new();

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

        private string _password;
        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        private string _confirmPassword;
        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public ObservableCollection<string> UserTypes { get; } = new()
        {
            "SystemAdmin", "User", "Customer", "SalesRep",
            "StoreManager", "Supplier", "Accountant"
        };

        private string _selectedUserType = "User";
        public string SelectedUserType
        {
            get => _selectedUserType;
            set => SetProperty(ref _selectedUserType, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public ICommand CreateCommand { get; }
        public ICommand CancelCommand { get; }

        public CreateUserViewModel()
        {
            CreateCommand = new RelayCommand(async () => await CreateAsync(), CanCreate);
            CancelCommand = new RelayCommand(() => Cancel());
        }

        private bool CanCreate()
        {
            return !string.IsNullOrWhiteSpace(Fullname) &&
                   !string.IsNullOrWhiteSpace(Email) &&
                   !string.IsNullOrWhiteSpace(Password) &&
                   Password == ConfirmPassword &&
                   !IsLoading;
        }

        private async Task CreateAsync()
        {
            if (Password != ConfirmPassword)
            {
                MessageBox.Show("كلمات المرور غير متطابقة", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IsLoading = true;

            var userDto = new UserPostDto
            {
                Fullname = Fullname,
                Email = Email,
                Password = Password,
                UserType = SelectedUserType,
                Phonenumber = PhoneNumber
            };

            var result = await _userService.CreateUserAsync(userDto);

            if (result != null)
            {
                MessageBox.Show($"تم إنشاء المستخدم {result.Fullname} بنجاح",
                    "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                ClearForm();
                NavigationService.NavigateToUsers();
            }
            else
            {
                MessageBox.Show("فشل إنشاء المستخدم. تأكد من البيانات وحاول مرة أخرى",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsLoading = false;
        }

        private void Cancel()
        {
            ClearForm();
            NavigationService.NavigateToUsers();
        }

        private void ClearForm()
        {
            Fullname = string.Empty;
            Email = string.Empty;
            PhoneNumber = string.Empty;
            Password = string.Empty;
            ConfirmPassword = string.Empty;
            SelectedUserType = "User";
        }
    }
}