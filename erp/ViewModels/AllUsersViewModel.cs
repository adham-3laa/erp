using erp.DTOS;
using erp.Services;
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;


using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace erp.ViewModels
{
    public class AllUsersViewModel : BaseViewModel
    {
        private readonly UserService _userService =
            new UserService(App.Api);
        private Timer _searchTimer;

        public AllUsersViewModel()
        {
            InitializeCommands();
            InitializeCollections();
            _ = LoadUsersAsync();
        }

        // ===================== Collections =====================
        public ObservableCollection<UserDto> Users { get; } = new();

        public ObservableCollection<string> UserTypes { get; private set; }
        public ObservableCollection<StatusOption> StatusOptions { get; private set; }

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
                id => ViewUserDetails(id),
                id => !IsLoading && !string.IsNullOrEmpty(id)
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
        }

        // ===================== Init =====================
        private void InitializeCollections()
        {
            UserTypes = new ObservableCollection<string>
            {
                "SystemAdmin","User","Customer","SalesRep",
                "StoreManager","Supplier","Accountant"
            };

            StatusOptions = new ObservableCollection<StatusOption>
            {
                new("جميع الحالات", null),
                new("نشط", true),
                new("غير نشط", false)
            };
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
                    Users.Add(user);
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

        private void ViewUserDetails(string userId)
        {
            var user = Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                MessageBox.Show(
                    $"الاسم: {user.Fullname}\n" +
                    $"البريد: {user.Email}\n" +
                    $"الهاتف: {user.Phonenumber}\n" +
                    $"النوع: {user.UserType}\n" +
                    $"الحالة: {(user.IsActive ? "نشط" : "غير نشط")}\n" +
                    $"تاريخ التسجيل: {user.DateOfCreation:yyyy-MM-dd}",
                    "تفاصيل المستخدم",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );
            }
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

    public record StatusOption(string Display, bool? Value);
}