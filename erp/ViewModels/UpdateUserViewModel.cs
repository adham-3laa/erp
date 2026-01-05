using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace erp.ViewModels
{
    public class UpdateUserViewModel : BaseViewModel
    {
        private readonly UserService _userService;
        private readonly string _userId;
        private UserDto _user;

        public UpdateUserViewModel(string userId)
        {
            _userId = userId;
            _userService = App.Users;

            InitializeUserTypes();

            SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
            CancelCommand = new RelayCommand(Cancel);

            LoadUserAsync();
        }

        // ================= PROPERTIES =================
        private string _fullname;
        public string Fullname
        {
            get => _fullname;
            set
            {
                SetProperty(ref _fullname, value);
                OnPropertyChanged(nameof(Initials));
                ((AsyncRelayCommand)SaveCommand).NotifyCanExecuteChanged();
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

        public ObservableCollection<UserTypeOption> UserTypes { get; private set; }

        private UserTypeOption _selectedUserType;
        public UserTypeOption SelectedUserType
        {
            get => _selectedUserType;
            set => SetProperty(ref _selectedUserType, value);
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                SetProperty(ref _isLoading, value);
                ((AsyncRelayCommand)SaveCommand).NotifyCanExecuteChanged();
            }
        }

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Fullname))
                    return "??";

                var parts = Fullname.Split(' ');
                return parts.Length >= 2
                    ? $"{parts[0][0]}{parts[1][0]}".ToUpper()
                    : Fullname.Length >= 2
                        ? $"{Fullname[0]}{Fullname[1]}".ToUpper()
                        : "??";
            }
        }

        // ================= COMMANDS =================
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        // ================= INITIALIZATION =================
        private void InitializeUserTypes()
        {
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
        }

        private async void LoadUserAsync()
        {
            IsLoading = true;

            _user = await _userService.GetUserByIdAsync(_userId);

            if (_user == null)
            {
                MessageBox.Show("فشل تحميل بيانات المستخدم", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                NavigationService.NavigateToUsers();
                return;
            }

       

            Fullname = _user.Fullname;
            IsActive = _user.IsActive;
            ImagePath = _user.ImagePath;

            // تعيين نوع المستخدم مع تجاهل الكيس والمسافات
            var userTypeOption = UserTypes
                .FirstOrDefault(x => string.Equals(x.Value?.Trim(), _user.UserType?.Trim(), StringComparison.OrdinalIgnoreCase));

            SelectedUserType = userTypeOption ?? UserTypes.FirstOrDefault();

            IsLoading = false;
        }

        private bool CanSave()
            => !string.IsNullOrWhiteSpace(Fullname) &&
               SelectedUserType != null &&
               !IsLoading;

        private async Task SaveAsync()
        {
            IsLoading = true;

            var dto = new UserUpdateDto
            {
                Fullname = Fullname,
                UserType = SelectedUserType?.Value,
                IsActive = IsActive,
                ImagePath = ImagePath
            };

            try
            {
                var result = await _userService.UpdateUserAsync(_userId, dto);
                IsLoading = false;

                if (result != null)
                {
                    MessageBox.Show("تم تحديث المستخدم بنجاح",
                        "نجاح", MessageBoxButton.OK, MessageBoxImage.Information);
                    NavigationService.NavigateToUsers();
                }
                else
                {
                    MessageBox.Show("فشل تحديث المستخدم",
                        "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                IsLoading = false;
                MessageBox.Show(ex.Message, "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            NavigationService.NavigateToUsers();
        }
        public class UserTypeOption
        {
            public string Text { get; set; }   // النص العربي
            public string Value { get; set; }  // القيمة الإنجليزية

            public UserTypeOption(string text, string value)
            {
                Text = text;
                Value = value;
            }

            // اختياري: لو عايز ToString يرجع النص العربي مباشرة
            public override string ToString() => Text;
        }

    }
}
