using CommunityToolkit.Mvvm.Input;
using erp.DTOS;
using erp.Services;
using System.Collections.ObjectModel;
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

        // ================= LOGIC =================

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
            SelectedUserType = _user.UserType;
            IsActive = _user.IsActive;
            ImagePath = _user.ImagePath;

            IsLoading = false;
        }

        private bool CanSave()
            => !string.IsNullOrWhiteSpace(Fullname) && !IsLoading;

        private async Task SaveAsync()
        {
            IsLoading = true;

            var dto = new UserUpdateDto
            {
                Fullname = Fullname,
                UserType = SelectedUserType,
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
                    // عرض رسالة فشل عامة
                    MessageBox.Show("فشل تحديث المستخدم",
                        "خطأ", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                IsLoading = false;

                // هنا هنعالج الـ error
                var msg = ex.Message ?? "";

                var jsonStart = msg.IndexOf('{');
                if (jsonStart >= 0)
                {
                    var json = msg.Substring(jsonStart);

                    try
                    {
                        using var doc = JsonDocument.Parse(json);
                        var root = doc.RootElement;

                        var statusCode = root.TryGetProperty("statusCode", out var sc) ? sc.GetInt32() : 0;
                        var apiMessage = root.TryGetProperty("message", out var m) ? (m.GetString() ?? "") : "";

                        if (statusCode == 500 &&
                            apiMessage.Contains("saving the entity changes", StringComparison.OrdinalIgnoreCase))
                        {
                            MessageBox.Show("الاسم أو البريد الإلكتروني مستخدم قبل كده ❌", "بيانات مكررة",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                    }
                    catch
                    {
                        // لو الـ JSON مش قابل للقراءة، هنكمل ونعرض رسالة خطأ عامة
                    }
                }

                // رسالة عامة لأي خطأ تاني
                MessageBox.Show("فشل تحديث المستخدم. تأكد من البيانات وحاول مرة أخرى.", "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                IsLoading = false;
                MessageBox.Show(ex.Message, "خطأ",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel()
        {
            NavigationService.NavigateToUsers();
        }
    }
}
