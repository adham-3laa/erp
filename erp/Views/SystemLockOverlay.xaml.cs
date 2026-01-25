using System.Windows;
using System.Windows.Input;
using erp.Services;
using erp.ViewModels;

namespace erp.Views
{
    /// <summary>
    /// Full-screen system lock overlay.
    /// Blocks all interaction until correct password is entered.
    /// </summary>
    public partial class SystemLockOverlay : Window
    {
        private readonly SystemLockOverlayViewModel _viewModel;

        public SystemLockOverlay(LockService lockService, System.Action onUnlocked)
        {
            InitializeComponent();
            
            _viewModel = new SystemLockOverlayViewModel(lockService, onUnlocked);
            DataContext = _viewModel;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Focus the password box when window loads
            PasswordBox.Focus();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            // Allow Enter key to trigger unlock
            if (e.Key == Key.Enter)
            {
                _viewModel.Password = PasswordBox.Password;
                
                if (_viewModel.UnlockCommand.CanExecute(null))
                {
                    _viewModel.UnlockCommand.Execute(null);
                }
            }
        }

        private void UnlockButton_Click(object sender, RoutedEventArgs e)
        {
            // Update password from PasswordBox
            _viewModel.Password = PasswordBox.Password;
        }

        /// <summary>
        /// Prevent closing the window via Alt+F4 or other means.
        /// Window can only be closed programmatically after unlock.
        /// </summary>
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            // Only allow closing if explicitly set (will be done after unlock)
            if (this.Tag?.ToString() != "AllowClose")
            {
                e.Cancel = true;
            }
            
            base.OnClosing(e);
        }
    }
}
