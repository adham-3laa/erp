using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace erp.ViewModels
{
    public class UpdateUserViewModel : BaseViewModel
    {
        private readonly UserService _userService = new();
        private readonly string _userId;
        private UserDto _user;

        private string _fullname;
        public string Fullname
        {
            get => _fullname;
            set
            {
                SetProperty(ref _fullname, value);
                OnPropertyChanged(nameof(Initials));
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set
            {
                SetProperty(ref _imagePath, value);
                OnPropertyChanged(nameof(Initials));
            }
        }

        public ObservableCollection<string> UserTypes { get; } = new()
        {
            "SystemAdmin", "User", "Customer", "SalesRep",
            "StoreManager", "Supplier", "Accountant"
        };

        private string _selectedUserType;
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

        public string Initials
        {
            get
            {
                if (string.IsNullOrEmpty(Fullname))
                    return "??";

                var parts = Fullname.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                    return $"{parts[0][0]}{parts[1][0]}".ToUpper();
                else if (parts.Length == 1 && parts[0].Length >= 2)
                    return $"{parts[0][0]}{parts[0][1]}".ToUpper();

                return "??";
            }
        }

        public ICommand LoadUserCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public UpdateUserViewModel(string userId)
        {
            _userId = userId;

            LoadUserCommand = new RelayCommand(async () => await LoadUserAsync());
            SaveCommand = new RelayCommand(async () => await SaveAsync(), CanSave);
            CancelCommand = new RelayCommand(() => Cancel());

            LoadUserCommand.Execute(null);
        }

        private async Task LoadUserAsync()
        {
            IsLoading = true;

            _user = await _userService.GetUserByIdAsync(_userId);

            if (_user != null)
            {
                Fullname = _user.Fullname;
                SelectedUserType = _user.UserType;
                IsActive = _user.IsActive;
                ImagePath = _user.ImagePath;
            }
            else
            {
                MessageBox.Show("فشل تحميل بيانات المستخدم", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.NavigateToUsers();
            }

            IsLoading = false;
        }

        private bool CanSave()
        {
            return !string.IsNullOrWhiteSpace(Fullname) && !IsLoading;
        }

        private async Task SaveAsync()
        {
            IsLoading = true;

            var updateDto = new UserUpdateDto
            {
                Fullname = Fullname,
                UserType = SelectedUserType,
                IsActive = IsActive,
                ImagePath = ImagePath
            };

            var result = await _userService.UpdateUserAsync(_userId, updateDto);

            if (result != null)
            {
                MessageBox.Show($"تم تحديث المستخدم {result.Fullname} بنجاح",
                    "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.NavigateToUsers();
            }
            else
            {
                MessageBox.Show("فشل تحديث المستخدم. تأكد من البيانات وحاول مرة أخرى",
                    "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            IsLoading = false;
        }

        private void Cancel()
        {
            NavigationService.NavigateToUsers();
        }
    }
}