using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using erp.Commands;
using erp.Services;

namespace erp.ViewModels
{
    /// <summary>
    /// ViewModel for the system lock overlay.
    /// One correct password = permanent unlock.
    /// 100 failed attempts = permanent lock.
    /// </summary>
    public class SystemLockOverlayViewModel : INotifyPropertyChanged
    {
        private readonly LockService _lockService;
        private readonly System.Action _onUnlocked;
        
        private string _password = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isProcessing = false;
        private int _remainingAttempts;
        private bool _isPermanentlyLocked = false;

        public event PropertyChangedEventHandler? PropertyChanged;

        public SystemLockOverlayViewModel(LockService lockService, System.Action onUnlocked)
        {
            _lockService = lockService;
            _onUnlocked = onUnlocked;
            
            UpdateRemainingAttempts();
            CheckIfPermanentlyLocked();
            
            UnlockCommand = new RelayCommand(ExecuteUnlock, CanExecuteUnlock);
        }

        /// <summary>
        /// The entered password.
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                    // Clear error when user types
                    if (!string.IsNullOrEmpty(_errorMessage))
                    {
                        ErrorMessage = string.Empty;
                    }
                }
            }
        }

        /// <summary>
        /// Error message for invalid password attempts.
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(HasError));
                }
            }
        }

        /// <summary>
        /// Indicates if there's an error message to display.
        /// </summary>
        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        /// <summary>
        /// Indicates if unlock operation is in progress.
        /// </summary>
        public bool IsProcessing
        {
            get => _isProcessing;
            set
            {
                if (_isProcessing != value)
                {
                    _isProcessing = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Remaining attempts before permanent lock.
        /// </summary>
        public int RemainingAttempts
        {
            get => _remainingAttempts;
            set
            {
                if (_remainingAttempts != value)
                {
                    _remainingAttempts = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(RemainingAttemptsText));
                }
            }
        }

        /// <summary>
        /// Indicates if system is permanently locked.
        /// </summary>
        public bool IsPermanentlyLocked
        {
            get => _isPermanentlyLocked;
            set
            {
                if (_isPermanentlyLocked != value)
                {
                    _isPermanentlyLocked = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PermanentLockMessage));
                }
            }
        }

        /// <summary>
        /// Display text for remaining attempts.
        /// </summary>
        public string RemainingAttemptsText => IsPermanentlyLocked 
            ? "⛔ النظام مقفل بشكل دائم" 
            : $"⚠️ باقي {RemainingAttempts} محاولة خاطئة قبل القفل الدائم";

        /// <summary>
        /// Message shown when permanently locked.
        /// </summary>
        public string PermanentLockMessage => "⛔ تم قفل النظام بشكل دائم بعد 100 محاولة خاطئة. لا يمكن فتح البرنامج.";

        /// <summary>
        /// Command to attempt unlock.
        /// </summary>
        public ICommand UnlockCommand { get; }

        private bool CanExecuteUnlock()
        {
            return !string.IsNullOrWhiteSpace(Password) && !IsProcessing && !IsPermanentlyLocked;
        }

        private void ExecuteUnlock()
        {
            IsProcessing = true;
            ErrorMessage = string.Empty;

            try
            {
                if (_lockService.ValidatePassword(Password))
                {
                    // Password correct - unlock permanently
                    _lockService.RecordSuccessfulAttempt();
                    
                    ErrorHandlingService.LogInfo("System unlocked permanently with correct password.");
                    
                    // Notify that unlock was successful
                    _onUnlocked?.Invoke();
                }
                else
                {
                    // Password incorrect - record failed attempt
                    int remaining = _lockService.RecordFailedAttempt();
                    
                    UpdateRemainingAttempts();
                    
                    if (remaining == 0)
                    {
                        // Permanently locked
                        IsPermanentlyLocked = true;
                        ErrorMessage = "❌ محاولة خاطئة! تم قفل النظام بشكل دائم بعد 100 محاولة خاطئة.";
                        ErrorHandlingService.LogInfo("System permanently locked after 100 failed attempts.");
                    }
                    else
                    {
                        // Still have attempts left
                        ErrorMessage = $"❌ كلمة المرور غير صحيحة. باقي {remaining} محاولة.";
                        ErrorHandlingService.LogInfo($"Failed attempt. {remaining} attempts remaining.");
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "حدث خطأ أثناء محاولة الفتح.";
                ErrorHandlingService.LogError(ex, "Error during unlock attempt");
            }
            finally
            {
                IsProcessing = false;
                Password = string.Empty; // Clear password field
            }
        }

        private void UpdateRemainingAttempts()
        {
            RemainingAttempts = _lockService.GetRemainingAttempts();
        }

        private void CheckIfPermanentlyLocked()
        {
            IsPermanentlyLocked = _lockService.IsPermanentlyLocked();
        }

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
