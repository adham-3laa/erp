using erp.DTOS;
using erp.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace erp.ViewModels.Returns
{
    public class ReturnsOrderItemsViewModel : BaseReturnsViewModel, INotifyPropertyChanged
    {
        private readonly ReturnsService _returnsService;

        public ReturnsOrderItemsViewModel(ReturnsService returnsService)
        {
            _returnsService = returnsService;
            OrderItems = new ObservableCollection<OrderItemForReturnDto>();
            LoadCommand = new AsyncRelayCommand(LoadOrderItemsAsyncWrapper, () => !IsBusy); // دالة وسيطة
        }

        public ObservableCollection<OrderItemForReturnDto> OrderItems { get; }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                (LoadCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged(); // Update command state
            }
        }

        private string _errorMessage;
        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadCommand { get; }

        // دالة وسيطة تستدعي LoadOrderItemsAsync مع المعلمة
        private async Task LoadOrderItemsAsyncWrapper()
        {
            var orderId = "YOUR_ORDER_ID"; // قم بتحديد الـ orderId بناءً على الحاجة
            await LoadOrderItemsAsync(orderId);  // استدعاء الدالة الأصلية مع الـ orderId
        }

        // دالة لتحميل بيانات الـ OrderItems بناءً على الـ orderId المدخل
        public async Task LoadOrderItemsAsync(string orderId)  // استقبال orderId كـ معلمة
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;

                // تحميل بيانات الـ OrderItems بناءً على الـ orderId
                var orderItems = await _returnsService.GetOrderItemsByOrderIdAsync(orderId);

                OrderItems.Clear();
                foreach (var item in orderItems)
                {
                    OrderItems.Add(item);
                }
            }
            catch (System.Exception ex)
            {
                ErrorMessage = "An error occurred while loading the order items: " + ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // RelayCommand to support async commands
    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;

        public async void Execute(object parameter) => await _execute();

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
