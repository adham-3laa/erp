using erp.DTOS;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.ComponentModel;

namespace erp.ViewModels
{
    public class AllUsersViewModel : BaseViewModel
    {
        private readonly UserService _userService = new UserService(App.Api);
        private Timer _searchTimer;

        public AllUsersViewModel()
        {
            InitializeCommands();
            InitializeCollections();
            _ = LoadUsersAsync();
        }

        // ===================== Collections =====================
        public ObservableCollection<WrappedUserDto> Users { get; } = new();
        public ObservableCollection<UserTypeOption> UserTypes { get; private set; }
        public ObservableCollection<StatusOption> StatusOptions { get; private set; }

        // قاموس لتحويل الإنجليزية إلى العربية
        private readonly Dictionary<string, string> _userTypeTranslations = new()
        {
            ["SystemAdmin"] = "مدير النظام",
            ["User"] = "مستخدم",
            ["Customer"] = "عميل",
            ["SalesRep"] = "مندوب مبيعات",
            ["StoreManager"] = "مدير مخزن",
            ["Supplier"] = "مورد",
            ["Accountant"] = "محاسب"
        };

        // ===================== Filters =====================
        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    DebounceSearch();
            }
        }

        private string _selectedUserType;
        public string SelectedUserType
        {
            get => _selectedUserType;
            set
            {
                if (SetProperty(ref _selectedUserType, value))
                {
                    CurrentPage = 1;
                    _ = LoadUsersAsync();
                }
            }
        }

        private bool? _selectedIsActive;
        public bool? SelectedIsActive
        {
            get => _selectedIsActive;
            set
            {
                if (SetProperty(ref _selectedIsActive, value))
                {
                    CurrentPage = 1;
                    _ = LoadUsersAsync();
                }
            }
        }

        // ===================== Pagination =====================
        private int _currentPage = 1;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (SetProperty(ref _currentPage, value))
                    _ = LoadUsersAsync();
            }
        }

        private int _totalPages;
        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        private int _totalCount;
        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public bool HasPreviousPage => CurrentPage > 1;
        public bool HasNextPage => CurrentPage < TotalPages;

        // ===================== UI State =====================
        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set => SetProperty(ref _isLoading, value);
        }

        public int ActiveCount => Users.Count(u => u.IsActive);

        // ===================== Commands =====================
        public RelayCommand LoadUsersCommand { get; private set; }
        public RelayCommand<string> ToggleStatusCommand { get; private set; }
        public RelayCommand<string> DeleteUserCommand { get; private set; }
        public RelayCommand<string> EditUserCommand { get; private set; }
        public RelayCommand<string> ViewUserCommand { get; private set; }
        public RelayCommand NextPageCommand { get; private set; }
        public RelayCommand PreviousPageCommand { get; private set; }
        public RelayCommand NavigateToCreateCommand { get; private set; }
        public RelayCommand<WrappedUserDto> ViewUserInvoicesCommand { get; private set; }

        private void InitializeCommands()
        {
            LoadUsersCommand = new RelayCommand(async () => await LoadUsersAsync());

            ToggleStatusCommand = new RelayCommand<string>(
                async id => await ToggleStatusAsync(id),
                id => !IsLoading
            );

            DeleteUserCommand = new RelayCommand<string>(
                async id => await DeleteUserAsync(id),
                id => !IsLoading
            );

            EditUserCommand = new RelayCommand<string>(
                id => NavigateToEditUser(id),
                id => !IsLoading && !string.IsNullOrEmpty(id)
            );

            ViewUserCommand = new RelayCommand<string>(
                OnViewUser,
                id => !IsLoading && !string.IsNullOrWhiteSpace(id)
            );

            NextPageCommand = new RelayCommand(
                () => CurrentPage++,
                () => HasNextPage && !IsLoading
            );

            PreviousPageCommand = new RelayCommand(
                () => CurrentPage--,
                () => HasPreviousPage && !IsLoading
            );

            NavigateToCreateCommand = new RelayCommand(
                () => NavigationService.NavigateToCreateUser()
            );

            ViewUserInvoicesCommand = new RelayCommand<WrappedUserDto>(
                user => NavigationService.NavigateToUserInvoices(user),
                user => user != null
            );
        }

        private void OnViewUser(string? userId)
        {
            if (IsLoading || string.IsNullOrWhiteSpace(userId))
                return;

            NavigationService.NavigateToUserProfile(userId);
        }

        // ===================== Init =====================
        private void InitializeCollections()
        {
            UserTypes = new ObservableCollection<UserTypeOption>
            {
                new UserTypeOption("جميع الأنواع", null),
                new UserTypeOption("مدير النظام", "SystemAdmin"),
                new UserTypeOption("مستخدم", "User"),
                new UserTypeOption("عميل", "Customer"),
                new UserTypeOption("مندوب مبيعات", "SalesRep"),
                new UserTypeOption("مدير مخزن", "StoreManager"),
                new UserTypeOption("مورد", "Supplier"),
                new UserTypeOption("محاسب", "Accountant")
            };

            StatusOptions = new ObservableCollection<StatusOption>
            {
                new StatusOption("جميع الحالات", null),
                new StatusOption("نشط", true),
                new StatusOption("غير نشط", false)
            };
        }

        // ===================== Helper Methods =====================
        public string GetUserTypeDisplay(string userType)
        {
            return _userTypeTranslations.TryGetValue(userType, out var arabicName)
                ? arabicName
                : userType;
        }

        // ===================== Core Logic =====================
        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;

                var res = await _userService.GetUsersAsync(
                    SearchText,
                    SelectedUserType,
                    SelectedIsActive,
                    CurrentPage,
                    20
                );

                if (res == null)
                {
                    Users.Clear();
                    TotalCount = 0;
                    TotalPages = 0;
                    return;
                }

                Users.Clear();
                foreach (var user in res.Users ?? Enumerable.Empty<UserDto>())
                {
                    // إنشاء WrappedUserDto الذي يحتوي على DisplayUserType
                    var wrappedUser = new WrappedUserDto(user, this);
                    Users.Add(wrappedUser);
                }

                TotalCount = res.TotalCount;
                TotalPages = res.FilteredCount == 0
                    ? 1
                    : (int)Math.Ceiling(res.FilteredCount / 20.0);

                OnPropertyChanged(nameof(ActiveCount));
                OnPropertyChanged(nameof(HasNextPage));
                OnPropertyChanged(nameof(HasPreviousPage));
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ToggleStatusAsync(string userId)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (user == null || IsLoading)
                return;

            var confirm = MessageBox.Show(
                $"هل تريد تغيير حالة المستخدم ({user.Fullname})؟",
                "تأكيد",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                IsLoading = true;

                var success = await _userService.ToggleUserStatusAsync(userId);

                if (success)
                {
                    // التغيير هنا فقط بعد نجاح الـ API
                    user.IsActive = !user.IsActive;
                    OnPropertyChanged(nameof(ActiveCount));
                }
                else
                {
                    MessageBox.Show("فشل تغيير حالة المستخدم");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "خطأ",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task DeleteUserAsync(string userId)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return;

            var confirm = MessageBox.Show(
                $"هل أنت متأكد من حذف المستخدم ({user.Fullname})؟",
                "تحذير",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning
            );

            if (confirm != MessageBoxResult.Yes)
                return;

            if (await _userService.DeleteUserAsync(userId))
            {
                Users.Remove(user);
                OnPropertyChanged(nameof(ActiveCount));
            }
            else
            {
                MessageBox.Show("فشل حذف المستخدم");
            }
        }

        private void NavigateToEditUser(string userId)
        {
            NavigationService.NavigateToUpdateUser(userId);
        }

        // ===================== Search Debounce =====================
        private void DebounceSearch()
        {
            _searchTimer?.Dispose();
            _searchTimer = new Timer(_ =>
            {
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    CurrentPage = 1;
                    await LoadUsersAsync();
                });
            }, null, 500, Timeout.Infinite);
        }
    }

    // ===================== WrappedUserDto Class =====================
    // ===================== WrappedUserDto Class =====================
    public class WrappedUserDto : INotifyPropertyChanged
    {
        private readonly UserDto _originalUser;
        private readonly AllUsersViewModel _viewModel;

        public WrappedUserDto(UserDto originalUser, AllUsersViewModel viewModel)
        {
            _originalUser = originalUser;
            _viewModel = viewModel;
        }

        // خاصية للعرض العربي
        public string DisplayUserType => _viewModel.GetUserTypeDisplay(_originalUser.UserType);

        // خصائص الـ UserDto الأصلية - جعلها قابلة للقراءة والكتابة
        private string _id;
        public string Id
        {
            get => _originalUser.Id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }

        private string _fullname;
        public string Fullname
        {
            get => _originalUser.Fullname;
            set
            {
                _fullname = value;
                OnPropertyChanged(nameof(Fullname));
            }
        }

        private string _username;
        public string Username
        {
            get => _originalUser.Username;
            set
            {
                _username = value;
                OnPropertyChanged(nameof(Username));
            }
        }

        private string _email;
        public string Email
        {
            get => _originalUser.Email;
            set
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }

        private string _salesRepId;
        public string? SalesRepId
        {
            get => _originalUser.SalesRepId;
            set
            {
                _salesRepId = value;
                OnPropertyChanged(nameof(SalesRepId));
            }
        }

        private string _phonenumber;
        public string Phonenumber
        {
            get => _originalUser.Phonenumber;
            set
            {
                _phonenumber = value;
                OnPropertyChanged(nameof(Phonenumber));
            }
        }

        private string _userType;
        public string UserType
        {
            get => _originalUser.UserType;
            set
            {
                _userType = value;
                OnPropertyChanged(nameof(UserType));
            }
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _originalUser.IsActive;
            set
            {
                _originalUser.IsActive = value;
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
            }
        }

        private string _imagePath;
        public string ImagePath
        {
            get => _originalUser.ImagePath;
            set
            {
                _imagePath = value;
                OnPropertyChanged(nameof(ImagePath));
            }
        }

        private DateTime _dateOfCreation;
        public DateTime DateOfCreation
        {
            get => _originalUser.DateOfCreation;
            set
            {
                _dateOfCreation = value;
                OnPropertyChanged(nameof(DateOfCreation));
            }
        }

        private int _farmsCount;
        public int FarmsCount
        {
            get => _originalUser.FarmsCount;
            set
            {
                _farmsCount = value;
                OnPropertyChanged(nameof(FarmsCount));
            }
        }

        private int _usernumber;
        public int usernumber
        {
            get => _originalUser.usernumber;
            set
            {
                _usernumber = value;
                OnPropertyChanged(nameof(usernumber));
            }
        }

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public record UserTypeOption(string Display, string Value);
    public record StatusOption(string Display, bool? Value);
}