using erp.DTOS;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace erp.ViewModels.Returns
{
    public class CreateReturnViewModel : INotifyPropertyChanged
    {
        private readonly ReturnsService _returnsService;

        private string _orderId;
        private string _errorMessage;
        private bool _isBusy;
        private CreateReturnItemDto _currentProduct;

        public CreateReturnViewModel(ReturnsService returnsService)
        {
            _returnsService = returnsService;

            Items = new ObservableCollection<CreateReturnItemDto>();
            CurrentProduct = new CreateReturnItemDto();

            CreateReturnCommand = new RelayCommand(
                async () => await CreateReturnAsync(),
                CanCreateReturn
            );
        }

        public ObservableCollection<CreateReturnItemDto> Items { get; }

        public CreateReturnItemDto CurrentProduct
        {
            get => _currentProduct;
            set
            {
                _currentProduct = value;
                OnPropertyChanged();
            }
        }

        public string OrderId
        {
            get => _orderId;
            set
            {
                _orderId = value;
                OnPropertyChanged();
                RaiseCommandState();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                RaiseCommandState();
            }
        }

        public ICommand CreateReturnCommand { get; }

        private bool CanCreateReturn()
        {
            return !string.IsNullOrWhiteSpace(OrderId)
                && Items.Count > 0
                && !IsBusy;
        }

        public void AddProduct()
        {
            Items.Add(CurrentProduct);
            RaiseCommandState();
        }

        public async Task<bool> CreateReturnAsync()
        {
            if (string.IsNullOrWhiteSpace(OrderId) || Items.Count == 0)
            {
                ErrorMessage = "من فضلك قم بإدخال رقم الطلب والعناصر";
                return false;
            }

            IsBusy = true;

            var request = new CreateReturnRequestDto
            {
                OrderId = OrderId,
                Items = new List<CreateReturnItemDto>(Items)
            };

            var success = await _returnsService.CreateReturnAsync(request);

            IsBusy = false;

            if (success)
            {
                Items.Clear();
                OrderId = string.Empty;
                CurrentProduct = new CreateReturnItemDto();
                ErrorMessage = string.Empty;
            }
            else
            {
                ErrorMessage = "فشل إنشاء طلب الإرجاع.";
            }

            RaiseCommandState();
            return success;
        }

        private void RaiseCommandState()
        {
            (CreateReturnCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // ================================
    // RelayCommand (مضبوط وآمن)
    // ================================
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;
        private readonly Func<bool> _canExecute;

        public RelayCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute?.Invoke() ?? true;
        }

        public async void Execute(object parameter)
        {
            if (!CanExecute(parameter))
                return;

            await _execute();
        }

        public event EventHandler CanExecuteChanged;

        public void RaiseCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
