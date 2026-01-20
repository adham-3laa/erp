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

        private string _orderId = "";
        public string OrderId
        {
            get => _orderId;
            set
            {
                SetProperty(ref _orderId, value);
                (LoadCommand as AsyncRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public ICommand LoadCommand { get; }

        // دالة وسيطة تستدعي LoadOrderItemsAsync مع المعلمة
        private async Task LoadOrderItemsAsyncWrapper()
        {
            if (string.IsNullOrWhiteSpace(OrderId))
            {
                ErrorMessage = "من فضلك أدخل رقم الطلب";
                return;
            }
            await LoadOrderItemsAsync(OrderId.Trim());
        }

        // دالة لتحميل بيانات الـ OrderItems بناءً على الـ orderId المدخل
        public async Task LoadOrderItemsAsync(string orderId)  // استقبال orderId كـ معلمة
        {
            try
            {
                IsBusy = true;
                ErrorMessage = string.Empty;
                
                if (string.IsNullOrWhiteSpace(orderId))
                {
                    ErrorMessage = "من فضلك أدخل رقم الطلب";
                    return;
                }

                // تحميل بيانات الـ OrderItems بناءً على الـ orderId
                var orderItems = await _returnsService.GetOrderItemsByOrderIdAsync(orderId.Trim());

                OrderItems.Clear();
                if (orderItems == null || orderItems.Count == 0)
                {
                    ErrorMessage = "لم يتم العثور على عناصر للطلب المحدد";
                    return;
                }
                
                foreach (var item in orderItems)
                {
                    OrderItems.Add(item);
                }
            }
            catch (System.Exception ex)
            {
                // تحسين رسالة الخطأ
                var errorMsg = ex.Message;
                if (errorMsg.Contains("404") || errorMsg.Contains("Not Found") || errorMsg.Contains("غير موجود"))
                {
                    ErrorMessage = $"الطلب برقم '{orderId}' غير موجود. تأكد من إدخال رقم الطلب الصحيح.";
                }
                else if (errorMsg.Contains("401") || errorMsg.Contains("Unauthorized"))
                {
                    ErrorMessage = "غير مصرح لك بالوصول. يرجى تسجيل الدخول مرة أخرى.";
                }
                else
                {
                    ErrorMessage = "حدث خطأ أثناء تحميل عناصر الطلب: " + errorMsg;
                }
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
