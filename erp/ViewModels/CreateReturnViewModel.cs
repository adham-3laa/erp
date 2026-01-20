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



        public ObservableCollection<CreateReturnItemDto> Items { get; }
        public ObservableCollection<string> FilteredProducts { get; } = new ObservableCollection<string>();

        private readonly InventoryService _inventoryService;

        public CreateReturnViewModel(ReturnsService returnsService, InventoryService inventoryService)
        {
            _returnsService = returnsService;
            _inventoryService = inventoryService;

            Items = new ObservableCollection<CreateReturnItemDto>();
            CurrentProduct = new CreateReturnItemDto();

            CreateReturnCommand = new RelayCommand(
                async () => await CreateReturnAsync()
            );
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                
                // Sync with the DTO
                if (CurrentProduct != null)
                {
                    CurrentProduct.ProductName = value;
                }

                _ = SearchProductsAsync(value);
            }
        }

        private async Task SearchProductsAsync(string query)
        {
           // FilteredProducts.Clear();
            
            if (string.IsNullOrWhiteSpace(query))
            {
               FilteredProducts.Clear();
               return;
            }

            try
            {
                var result = await _inventoryService.GetAutocompleteProductsAsync(query);
                
                FilteredProducts.Clear();
                foreach (var item in result)
                {
                    FilteredProducts.Add(item.Name);
                }
            }
            catch
            {
                // handle error or ignore
            }
        }

        public CreateReturnItemDto CurrentProduct
        {
            get => _currentProduct;
            set
            {
                _currentProduct = value;
                // When we reset the product (e.g. after adding), clear the search text too
                _searchText = _currentProduct?.ProductName ?? ""; 
                OnPropertyChanged();
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public string OrderId
        {
            get => _orderId;
            set
            {
                _orderId = value;
                OnPropertyChanged();
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
            }
        }

        public ICommand CreateReturnCommand { get; }
        public void AddProduct()
        {
            Items.Add(CurrentProduct);
        }

        public bool IsCurrentProductValid()
        {
            return !string.IsNullOrWhiteSpace(CurrentProduct.ProductName)
                && CurrentProduct.Quantity > 0
                && !string.IsNullOrWhiteSpace(CurrentProduct.Reason);
        }

        public async Task CreateReturnAsync()
        {
            ErrorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(OrderId))
            {
                ErrorMessage = "رقم الطلب مطلوب";
                return;
            }

            // تحويل OrderId إلى int (ordercode)
            if (!int.TryParse(OrderId.Trim(), out int orderCode))
            {
                ErrorMessage = "رقم الطلب يجب أن يكون رقماً صحيحاً";
                return;
            }

            // لو المستخدم كتب منتج ولسه متضافش
            if (IsCurrentProductValid())
            {
                Items.Add(CurrentProduct);
                CurrentProduct = new CreateReturnItemDto();
            }

            if (Items.Count == 0)
            {
                ErrorMessage = "أدخل منتج واحد على الأقل";
                return;
            }

            IsBusy = true;

            var request = new CreateReturnRequestDto
            {
                OrderCode = orderCode,
                Items = new List<CreateReturnItemDto>(Items)
            };

            var success = await _returnsService.CreateReturnAsync(request);

            IsBusy = false;

            if (success)
            {
                Items.Clear();
                OrderId = string.Empty;
                CurrentProduct = new CreateReturnItemDto();
                ErrorMessage = "تم إنشاء طلب الإرجاع بنجاح";
            }
            else
            {
                ErrorMessage = "فشل إنشاء طلب الإرجاع";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // =============================
    // RelayCommand
    // =============================
    public class RelayCommand : ICommand
    {
        private readonly Func<Task> _execute;

        public RelayCommand(Func<Task> execute)
        {
            _execute = execute;
        }

        public bool CanExecute(object parameter) => true;

        public async void Execute(object parameter)
        {
            await _execute();
        }

        public event EventHandler CanExecuteChanged;
    }
}
