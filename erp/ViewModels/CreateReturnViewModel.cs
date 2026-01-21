using erp.Commands;
using erp.DTOS;
using erp.Services;
using erp;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace erp.ViewModels.Returns
{
    public class CreateReturnViewModel : INotifyPropertyChanged
    {
        private readonly ReturnsService _returnsService;
        private readonly InventoryService _inventoryService;
        private readonly UserService _userService;

        // ================== MODE SELECTION ==================
        private bool _isCustomerReturn = true;
        public bool IsCustomerReturn
        {
            get => _isCustomerReturn;
            set
            {
                if (_isCustomerReturn != value)
                {
                    _isCustomerReturn = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSupplierReturn));
                    ResetForm();
                }
            }
        }
        public bool IsSupplierReturn
        {
            get => !_isCustomerReturn;
            set => IsCustomerReturn = !value;
        }

        // ================== COMMON ==================
        public ObservableCollection<CreateReturnItemDto> ItemsToReturn { get; } = new();
        
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); CommandManager.InvalidateRequerySuggested(); }
        }

        // ================== CUSTOMER RETURN UI ==================
        private string _orderCode;
        public string OrderCode
        {
            get => _orderCode;
            set { _orderCode = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SelectableOrderItem> OrderItems { get; } = new();
        public ICommand FetchOrderCommand { get; }

        // ================== SUPPLIER RETURN UI ==================
        private string _supplierName = "";
        public string SupplierName
        {
            get => _supplierName;
            set
            {
                _supplierName = value;
                OnPropertyChanged();
                FilterSupplierSuggestions(value);
            }
        }

        private List<string> _allUsersCache = new();
        public ObservableCollection<string> FilteredSupplierSuggestions { get; } = new();
        
        private bool _isSupplierSuggestionOpen;
        public bool IsSupplierSuggestionOpen
        {
            get => _isSupplierSuggestionOpen;
            set { _isSupplierSuggestionOpen = value; OnPropertyChanged(); }
        }

        private string _selectedSupplierSuggestion;
        public string SelectedSupplierSuggestion
        {
            get => _selectedSupplierSuggestion;
            set
            {
                _selectedSupplierSuggestion = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    SupplierName = value;
                    IsSupplierSuggestionOpen = false;
                }
            }
        }

        private CreateReturnItemDto _currentSupplierItem = new();
        public CreateReturnItemDto CurrentSupplierItem
        {
            get => _currentSupplierItem;
            set { _currentSupplierItem = value; OnPropertyChanged(); }
        }

        public ObservableCollection<string> ProductSuggestions { get; } = new();
        private string _productSearchText;
        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                _productSearchText = value;
                OnPropertyChanged();
                if (CurrentSupplierItem != null) CurrentSupplierItem.ProductName = value;
                _ = SearchProductsAsync(value);
            }
        }

        public ICommand AddSupplierItemCommand { get; }
        public ICommand RemoveItemCommand { get; }

        // ================== ACTIONS ==================
        public ICommand SubmitReturnCommand { get; }

        // ================== CONSTRUCTOR ==================
        public CreateReturnViewModel(ReturnsService returnsService, InventoryService inventoryService)
        {
            _returnsService = returnsService;
            _inventoryService = inventoryService;
            _userService = App.Users;

            FetchOrderCommand = new RelayCommand(async () => await FetchOrderItemsAsync());
            AddSupplierItemCommand = new RelayCommand(AddSupplierItem);
            RemoveItemCommand = new RelayCommand<CreateReturnItemDto>(RemoveItem);
            SubmitReturnCommand = new RelayCommand(async () => await SubmitReturnAsync());
            
            // Load users for autocomplete suggestions
            _ = LoadUsersForAutocompleteAsync();
        }

        private void ResetForm()
        {
            ItemsToReturn.Clear();
            OrderItems.Clear();
            OrderCode = "";
            SupplierName = "";
            CurrentSupplierItem = new CreateReturnItemDto();
            ProductSearchText = "";
            ProductSuggestions.Clear();
        }

        // ================== CUSTOMER LOGIC ==================
        private async Task FetchOrderItemsAsync()
        {
            if (string.IsNullOrWhiteSpace(OrderCode))
            {
                ErrorHandlingService.ShowWarning("أدخل رقم الطلب أولاً");
                return;
            }

            IsBusy = true;
            try
            {
                var items = await _returnsService.GetOrderItemsByOrderIdAsync(OrderCode);
                OrderItems.Clear();
                foreach (var item in items)
                {
                    OrderItems.Add(new SelectableOrderItem(item));
                }

                if (items.Count == 0)
                {
                    ErrorHandlingService.ShowWarning("لا توجد عناصر لهذا الطلب");
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.ShowError(ErrorHandlingService.GetUserFriendlyMessage(ex, "فشل تحميل عناصر الطلب"));
            }
            finally
            {
                IsBusy = false;
            }
        }

        // ================== SUPPLIER LOGIC ==================
        private async Task LoadUsersForAutocompleteAsync()
        {
            try
            {
                var response = await _userService.GetUsersAsync(pageSize: 1000);
                if (response?.Users != null)
                {
                    _allUsersCache = response.Users
                        .Where(u => !string.IsNullOrWhiteSpace(u.Fullname))
                        .Select(u => u.Fullname)
                        .Distinct()
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "Autocomplete load failed");
            }
        }

        private void FilterSupplierSuggestions(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                IsSupplierSuggestionOpen = false;
                FilteredSupplierSuggestions.Clear();
                return;
            }

            var matches = _allUsersCache
                .Where(name => name.Contains(input, StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name.Length)
                .Take(10)
                .ToList();

            FilteredSupplierSuggestions.Clear();
            foreach (var m in matches) FilteredSupplierSuggestions.Add(m);

            IsSupplierSuggestionOpen = FilteredSupplierSuggestions.Any();
        }

        private async Task SearchProductsAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                ProductSuggestions.Clear();
                return;
            }

            try
            {
                var result = await _inventoryService.GetAutocompleteProductsAsync(query);
                ProductSuggestions.Clear();
                if (result != null)
                {
                    foreach (var item in result) ProductSuggestions.Add(item.Name);
                }
            }
            catch { /* Ignore */ }
        }

        private void AddSupplierItem()
        {
            if (string.IsNullOrWhiteSpace(CurrentSupplierItem.ProductName) || CurrentSupplierItem.Quantity <= 0)
            {
                ErrorHandlingService.ShowWarning("يرجى إدخال اسم المنتج والكمية بشكل صحيح");
                return;
            }
            if (string.IsNullOrWhiteSpace(CurrentSupplierItem.Reason))
            {
                ErrorHandlingService.ShowWarning("يرجى ذكر سبب الإرجاع");
                return;
            }

            ItemsToReturn.Add(CurrentSupplierItem);
            
            CurrentSupplierItem = new CreateReturnItemDto();
            ProductSearchText = "";
        }

        private void RemoveItem(CreateReturnItemDto item)
        {
            if (ItemsToReturn.Contains(item))
                ItemsToReturn.Remove(item);
        }

        // ================== SUBMIT LOGIC ==================
        private async Task SubmitReturnAsync()
        {
            if (IsBusy) return;

            if (IsCustomerReturn)
            {
                await SubmitCustomerReturn();
            }
            else
            {
                await SubmitSupplierReturn();
            }
        }

        private async Task SubmitCustomerReturn()
        {
            var selectedItems = OrderItems.Where(i => i.IsSelected).ToList();
            if(!selectedItems.Any())
            {
                ErrorHandlingService.ShowWarning("اختر منتجات للإرجاع من القائمة");
                return;
            }
            
            foreach(var item in selectedItems)
            {
                if(item.ReturnQuantity <= 0)
                {
                     ErrorHandlingService.ShowWarning($"الكمية غير صالحة للمنتج: {item.Dto.Productname}");
                     return;
                }
                if (item.ReturnQuantity > item.Dto.Quantity)
                {
                    ErrorHandlingService.ShowWarning($"لا يمكن إرجاع كمية ({item.ReturnQuantity}) أكبر من المشتراة ({item.Dto.Quantity}) للمنتج: {item.Dto.Productname}");
                    return;
                }
                if(string.IsNullOrWhiteSpace(item.Reason))
                {
                    ErrorHandlingService.ShowWarning($"يجب كتابة سبب الإرجاع للمنتج: {item.Dto.Productname}");
                    return;
                }
            }

            if (!int.TryParse(OrderCode, out int codeInt))
            {
                 ErrorHandlingService.ShowWarning("رقم الطلب غير صالح");
                 return;
            }

            var request = new CreateReturnRequestDto
            {
                OrderCode = codeInt,
                Items = selectedItems.Select(i => new CreateReturnItemDto
                {
                    ProductName = i.Dto.Productname,
                    Quantity = i.ReturnQuantity,
                    Reason = i.Reason
                }).ToList()
            };

            if (ErrorHandlingService.Confirm("هل أنت متأكد من إنشاء طلب الإرجاع؟", "تأكيد"))
            {
                IsBusy = true;
                var (success, msg) = await _returnsService.CreateReturnAsync(request);
                IsBusy = false;

                if (success)
                {
                     ErrorHandlingService.ShowSuccess("تم إنشاء طلب الإرجاع بنجاح");
                     ResetForm();
                }
                else
                {
                    ErrorHandlingService.ShowError(msg ?? "حدث خطأ غير معروف");
                }
            }
        }

        private async Task SubmitSupplierReturn()
        {
            if (string.IsNullOrWhiteSpace(SupplierName))
            {
                ErrorHandlingService.ShowWarning("اسم المورد مطلوب");
                return;
            }
            if (ItemsToReturn.Count == 0)
            {
                ErrorHandlingService.ShowWarning("أضف منتجات للقائمة أولاً");
                return;
            }

            var request = new ReturnToSupplierRequestDto
            {
                SupplierName = SupplierName,
                Items = ItemsToReturn.ToList()
            };

            if (ErrorHandlingService.Confirm($"هل أنت متأكد من إرجاع {ItemsToReturn.Count} منتجات للمورد {SupplierName}؟", "تأكيد"))
            {
                IsBusy = true;
                var (success, msg) = await _returnsService.ReturnToSupplierAsync(request);
                IsBusy = false;

                if (success)
                {
                    ErrorHandlingService.ShowSuccess("تم إرجاع المنتجات للمورد بنجاح");
                    ResetForm();
                }
                else
                {
                    ErrorHandlingService.ShowError(msg ?? "حدث خطأ");
                }
            }
        }

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class SelectableOrderItem : INotifyPropertyChanged
    {
        public OrderItemForReturnDto Dto { get; }
        private bool _isSelected;
        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnPropertyChanged(); } }

        private int _returnQuantity;
        public int ReturnQuantity { get => _returnQuantity; set { _returnQuantity = value; OnPropertyChanged(); } }

        private string _reason = "";
        public string Reason { get => _reason; set { _reason = value; OnPropertyChanged(); } }

        public SelectableOrderItem(OrderItemForReturnDto dto)
        {
            Dto = dto;
            ReturnQuantity = dto.Quantity; 
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    // Note: RelayCommand and RelayCommand<T> are now in erp.Commands namespace
}
