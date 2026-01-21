using erp.Commands;
using erp.DTOS;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace erp.ViewModels.Returns
{
    /// <summary>
    /// Enhanced Return ViewModel with step-by-step flow, proper async handling,
    /// strict Arabic error messages, and improved UX
    /// </summary>
    public class EnhancedCreateReturnViewModel : INotifyPropertyChanged
    {
        #region Services
        private readonly ReturnsService _returnsService;
        private readonly InventoryService _inventoryService;
        private readonly UserService _userService;
        #endregion

        #region Return Type Selection
        private ReturnType _selectedReturnType = ReturnType.Customer;
        public ReturnType SelectedReturnType
        {
            get => _selectedReturnType;
            set
            {
                if (_selectedReturnType != value)
                {
                    _selectedReturnType = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCustomerReturn));
                    OnPropertyChanged(nameof(IsSupplierReturn));
                    ResetAllSteps();
                }
            }
        }

        public bool IsCustomerReturn
        {
            get => _selectedReturnType == ReturnType.Customer;
            set { if (value) SelectedReturnType = ReturnType.Customer; }
        }

        public bool IsSupplierReturn
        {
            get => _selectedReturnType == ReturnType.Supplier;
            set { if (value) SelectedReturnType = ReturnType.Supplier; }
        }
        #endregion

        #region Step Navigation
        private int _currentStep = 1;
        public int CurrentStep
        {
            get => _currentStep;
            set
            {
                if (_currentStep != value && value >= 1 && value <= 3)
                {
                    _currentStep = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CanGoBack));
                    OnPropertyChanged(nameof(CanGoNext));
                    OnPropertyChanged(nameof(CanSubmit));
                    OnPropertyChanged(nameof(CurrentStepTitle));
                    OnPropertyChanged(nameof(CurrentStepDescription));
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool CanGoBack => CurrentStep > 1 && !IsBusy;
        public bool CanGoNext => ValidateCurrentStep() && !IsBusy && CurrentStep < 3;
        public bool CanSubmit => CurrentStep == 3 && ValidateAllSteps() && !IsBusy;

        public string CurrentStepTitle => CurrentStep switch
        {
            1 => IsCustomerReturn ? "الخطوة ١: إدخال رقم الطلب" : "الخطوة ١: اختيار المورد",
            2 => "الخطوة ٢: اختيار المنتجات وتحديد الكميات",
            3 => "الخطوة ٣: مراجعة وتأكيد",
            _ => ""
        };

        public string CurrentStepDescription => CurrentStep switch
        {
            1 => IsCustomerReturn 
                ? "أدخل رقم الطلب للبحث عن المنتجات المتاحة للإرجاع" 
                : "اختر اسم المورد الذي ستقوم بإرجاع المنتجات إليه",
            2 => "حدد المنتجات، الكميات، وسبب الإرجاع",
            3 => "راجع بيانات الإرجاع بعناية ثم اضغط تأكيد",
            _ => ""
        };
        #endregion

        #region Loading & Status
        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoBack));
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanSubmit));
                OnPropertyChanged(nameof(IsInputEnabled));
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsInputEnabled => !IsBusy;

        private string _loadingMessage = "";
        public string LoadingMessage
        {
            get => _loadingMessage;
            set { _loadingMessage = value; OnPropertyChanged(); }
        }

        private string _statusMessage = "";
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasStatusMessage)); }
        }

        public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);

        private StatusType _statusType = StatusType.None;
        public StatusType CurrentStatusType
        {
            get => _statusType;
            set { _statusType = value; OnPropertyChanged(); }
        }

        private bool _isSuccess;
        public bool IsSuccess
        {
            get => _isSuccess;
            set { _isSuccess = value; OnPropertyChanged(); }
        }
        #endregion

        #region Customer Return Properties
        private string _orderCode = "";
        public string OrderCode
        {
            get => _orderCode;
            set
            {
                _orderCode = value?.Trim() ?? "";
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoNext));
                ClearStatus();
            }
        }

        public ObservableCollection<SelectableOrderItemEnhanced> OrderItems { get; } = new();
        
        public int SelectedItemsCount => OrderItems.Count(i => i.IsSelected);
        public int TotalItemsCount => OrderItems.Count;
        #endregion

        #region Supplier Return Properties
        private string _supplierName = "";
        public string SupplierName
        {
            get => _supplierName;
            set
            {
                _supplierName = value?.Trim() ?? "";
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoNext));
                FilterSupplierSuggestions(value);
                ClearStatus();
            }
        }

        private string _supplierInvoiceCode = "";
        public string SupplierInvoiceCode
        {
            get => _supplierInvoiceCode;
            set
            {
                _supplierInvoiceCode = value?.Trim() ?? "";
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanGoNext));
                ClearStatus();
            }
        }

        // Collection for products loaded from invoice (selectable)
        public ObservableCollection<SelectableSupplierInvoiceProduct> SupplierInvoiceProducts { get; } = new();

        private List<string> _allSuppliersCache = new();
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

        // Current item being added for supplier return
        private SupplierReturnItem _currentSupplierItem = new();
        public SupplierReturnItem CurrentSupplierItem
        {
            get => _currentSupplierItem;
            set { _currentSupplierItem = value; OnPropertyChanged(); }
        }

        public ObservableCollection<SupplierReturnItem> SupplierReturnItems { get; } = new();

        // Product search for supplier returns
        public ObservableCollection<string> ProductSuggestions { get; } = new();
        
        private bool _isProductSuggestionOpen;
        public bool IsProductSuggestionOpen
        {
            get => _isProductSuggestionOpen;
            set { _isProductSuggestionOpen = value; OnPropertyChanged(); }
        }

        private string _selectedProductSuggestion;
        public string SelectedProductSuggestion
        {
            get => _selectedProductSuggestion;
            set
            {
                _selectedProductSuggestion = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(value))
                {
                    ProductSearchText = value;
                    IsProductSuggestionOpen = false;
                }
            }
        }

        private string _productSearchText = "";
        public string ProductSearchText
        {
            get => _productSearchText;
            set
            {
                _productSearchText = value;
                OnPropertyChanged();
                if (CurrentSupplierItem != null)
                    CurrentSupplierItem.ProductName = value;
                
                if (string.IsNullOrEmpty(value))
                {
                    IsProductSuggestionOpen = false;
                    ProductSuggestions.Clear();
                }
                else
                {
                    _ = SearchProductsAsync(value);
                }
            }
        }

        public int SelectedSupplierInvoiceItemsCount => SupplierInvoiceProducts.Count(i => i.IsSelected);
        public int TotalSupplierInvoiceItemsCount => SupplierInvoiceProducts.Count;
        #endregion

        #region Commands
        public ICommand FetchOrderCommand { get; }
        public ICommand FetchSupplierInvoiceCommand { get; }
        public ICommand NextStepCommand { get; }
        public ICommand PreviousStepCommand { get; }
        public ICommand SubmitReturnCommand { get; }
        public ICommand AddSupplierItemCommand { get; }
        public ICommand RemoveSupplierItemCommand { get; }
        public ICommand SelectAllItemsCommand { get; }
        public ICommand DeselectAllItemsCommand { get; }
        public ICommand SelectAllSupplierInvoiceItemsCommand { get; }
        public ICommand DeselectAllSupplierInvoiceItemsCommand { get; }
        public ICommand StartNewReturnCommand { get; }
        #endregion

        #region Constructor
        public EnhancedCreateReturnViewModel(ReturnsService returnsService, InventoryService inventoryService)
        {
            _returnsService = returnsService ?? throw new ArgumentNullException(nameof(returnsService));
            _inventoryService = inventoryService ?? throw new ArgumentNullException(nameof(inventoryService));
            _userService = App.Users;

            // Initialize commands
            FetchOrderCommand = new AsyncRelayCommand(FetchOrderItemsAsync, () => !IsBusy && !string.IsNullOrWhiteSpace(OrderCode));
            FetchSupplierInvoiceCommand = new AsyncRelayCommand(FetchSupplierInvoiceProductsAsync, () => !IsBusy && !string.IsNullOrWhiteSpace(SupplierInvoiceCode));
            NextStepCommand = new AsyncRelayCommand(GoToNextStepAsync, () => CanGoNext);
            PreviousStepCommand = new RelayCommand(GoToPreviousStep, () => CanGoBack);
            SubmitReturnCommand = new AsyncRelayCommand(SubmitReturnAsync, () => CanSubmit);
            AddSupplierItemCommand = new RelayCommand(AddSupplierItem, () => !IsBusy);
            RemoveSupplierItemCommand = new RelayCommand<SupplierReturnItem>(RemoveSupplierItem);
            SelectAllItemsCommand = new RelayCommand(SelectAllItems);
            DeselectAllItemsCommand = new RelayCommand(DeselectAllItems);
            SelectAllSupplierInvoiceItemsCommand = new RelayCommand(SelectAllSupplierInvoiceItems);
            DeselectAllSupplierInvoiceItemsCommand = new RelayCommand(DeselectAllSupplierInvoiceItems);
            StartNewReturnCommand = new RelayCommand(StartNewReturn);

            // Load suppliers for autocomplete
            _ = LoadSuppliersAsync();
        }
        #endregion

        #region Step Validation
        private bool ValidateCurrentStep()
        {
            return CurrentStep switch
            {
                1 => ValidateStep1(),
                2 => ValidateStep2(),
                3 => ValidateAllSteps(),
                _ => false
            };
        }

        private bool ValidateStep1()
        {
            if (IsCustomerReturn)
            {
                return !string.IsNullOrWhiteSpace(OrderCode) && OrderItems.Count > 0;
            }
            else
            {
                // For supplier return, validate we have supplier name AND invoice code with loaded products
                return !string.IsNullOrWhiteSpace(SupplierName) && 
                       !string.IsNullOrWhiteSpace(SupplierInvoiceCode) &&
                       SupplierInvoiceProducts.Count > 0;
            }
        }

        private bool ValidateStep2()
        {
            if (IsCustomerReturn)
            {
                var selected = OrderItems.Where(i => i.IsSelected).ToList();
                if (selected.Count == 0) return false;

                return selected.All(i => 
                    i.ReturnQuantity > 0 && 
                    i.ReturnQuantity <= i.Dto.Quantity && 
                    !string.IsNullOrWhiteSpace(i.Reason));
            }
            else
            {
                // Validate selected products from invoice
                var selectedInvoiceProducts = SupplierInvoiceProducts.Where(p => p.IsSelected).ToList();
                if (selectedInvoiceProducts.Count == 0) return false;

                return selectedInvoiceProducts.All(i => 
                    i.ReturnQuantity > 0 && 
                    i.ReturnQuantity <= i.Dto.Quantity && 
                    !string.IsNullOrWhiteSpace(i.Reason));
            }
        }

        private bool ValidateAllSteps()
        {
            return ValidateStep1() && ValidateStep2();
        }
        #endregion

        #region Navigation Methods
        private async Task GoToNextStepAsync()
        {
            ClearStatus();

            // If moving from step 1 to 2 for customer return, fetch items first
            if (CurrentStep == 1 && IsCustomerReturn && OrderItems.Count == 0)
            {
                await FetchOrderItemsAsync();
                if (OrderItems.Count == 0) return; // Don't advance if no items found
            }

            if (ValidateCurrentStep())
            {
                CurrentStep++;
            }
            else
            {
                ShowValidationError();
            }
        }

        private void GoToPreviousStep()
        {
            if (CurrentStep > 1)
            {
                ClearStatus();
                CurrentStep--;
            }
        }

        private void ShowValidationError()
        {
            var message = CurrentStep switch
            {
                1 when IsCustomerReturn => "يرجى إدخال رقم الطلب وجلب المنتجات أولاً",
                1 when IsSupplierReturn => "يرجى إدخال اسم المورد ورقم الفاتورة وتحميل المنتجات",
                2 when IsCustomerReturn => "يرجى اختيار منتج واحد على الأقل وتحديد الكمية والسبب",
                2 when IsSupplierReturn => "يرجى اختيار منتج واحد على الأقل من الفاتورة وتحديد الكمية والسبب",
                _ => "يرجى إكمال جميع البيانات المطلوبة"
            };
            SetStatus(message, StatusType.Warning);
        }
        #endregion

        #region Customer Return Logic
        private async Task FetchOrderItemsAsync()
        {
            if (string.IsNullOrWhiteSpace(OrderCode))
            {
                SetStatus("يرجى إدخال رقم الطلب أولاً", StatusType.Warning);
                return;
            }

            IsBusy = true;
            LoadingMessage = "جاري البحث عن منتجات الطلب...";
            ClearStatus();

            try
            {
                var items = await _returnsService.GetOrderItemsByOrderIdAsync(OrderCode);
                
                OrderItems.Clear();
                foreach (var item in items)
                {
                    var selectable = new SelectableOrderItemEnhanced(item);
                    selectable.PropertyChanged += (s, e) => 
                    {
                        OnPropertyChanged(nameof(SelectedItemsCount));
                        OnPropertyChanged(nameof(CanGoNext));
                        OnPropertyChanged(nameof(CanSubmit));
                    };
                    OrderItems.Add(selectable);
                }

                OnPropertyChanged(nameof(TotalItemsCount));
                OnPropertyChanged(nameof(SelectedItemsCount));

                if (items.Count == 0)
                {
                    SetStatus("لا توجد منتجات متاحة للإرجاع في هذا الطلب", StatusType.Warning);
                }
                else
                {
                    SetStatus($"تم العثور على {items.Count} منتج(ات)", StatusType.Success);
                }
            }
            catch (ServiceException ex)
            {
                ErrorHandlingService.LogError(ex, "FetchOrderItemsAsync");
                SetStatus(ex.Message, StatusType.Error);
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "FetchOrderItemsAsync");
                SetStatus(HandleException(ex, "فشل في تحميل منتجات الطلب"), StatusType.Error);
            }
            finally
            {
                IsBusy = false;
                LoadingMessage = "";
            }
        }

        private void SelectAllItems()
        {
            foreach (var item in OrderItems)
                item.IsSelected = true;
            OnPropertyChanged(nameof(SelectedItemsCount));
        }

        private void DeselectAllItems()
        {
            foreach (var item in OrderItems)
                item.IsSelected = false;
            OnPropertyChanged(nameof(SelectedItemsCount));
        }

        private void SelectAllSupplierInvoiceItems()
        {
            foreach (var item in SupplierInvoiceProducts)
                item.IsSelected = true;
            OnPropertyChanged(nameof(SelectedSupplierInvoiceItemsCount));
        }

        private void DeselectAllSupplierInvoiceItems()
        {
            foreach (var item in SupplierInvoiceProducts)
                item.IsSelected = false;
            OnPropertyChanged(nameof(SelectedSupplierInvoiceItemsCount));
        }
        #endregion

        #region Supplier Return Logic
        private async Task LoadSuppliersAsync()
        {
            try
            {
                var response = await _userService.GetUsersAsync(pageSize: 1000);
                if (response?.Users != null)
                {
                    _allSuppliersCache = response.Users
                        .Where(u => !string.IsNullOrWhiteSpace(u.Fullname))
                        .Select(u => u.Fullname)
                        .Distinct()
                        .OrderBy(n => n)
                        .ToList();
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "LoadSuppliersAsync");
                // Silent fail - autocomplete is non-critical
            }
        }

        private async Task FetchSupplierInvoiceProductsAsync()
        {
            if (string.IsNullOrWhiteSpace(SupplierInvoiceCode))
            {
                SetStatus("يرجى إدخال رقم الفاتورة أولاً", StatusType.Warning);
                return;
            }

            if (!int.TryParse(SupplierInvoiceCode, out int invoiceCode))
            {
                SetStatus("رقم الفاتورة غير صالح", StatusType.Error);
                return;
            }

            IsBusy = true;
            LoadingMessage = "جاري البحث عن منتجات الفاتورة...";
            ClearStatus();

            try
            {
                var products = await _returnsService.GetSupplierInvoiceProductsAsync(invoiceCode);
                
                SupplierInvoiceProducts.Clear();
                foreach (var product in products)
                {
                    var selectable = new SelectableSupplierInvoiceProduct(product);
                    selectable.PropertyChanged += (s, e) => 
                    {
                        OnPropertyChanged(nameof(SelectedSupplierInvoiceItemsCount));
                        OnPropertyChanged(nameof(CanGoNext));
                        OnPropertyChanged(nameof(CanSubmit));
                    };
                    SupplierInvoiceProducts.Add(selectable);
                }

                OnPropertyChanged(nameof(TotalSupplierInvoiceItemsCount));
                OnPropertyChanged(nameof(SelectedSupplierInvoiceItemsCount));

                if (products.Count == 0)
                {
                    SetStatus("لا توجد منتجات في هذه الفاتورة", StatusType.Warning);
                }
                else
                {
                    SetStatus($"تم العثور على {products.Count} منتج(ات)", StatusType.Success);
                }
            }
            catch (ServiceException ex)
            {
                ErrorHandlingService.LogError(ex, "FetchSupplierInvoiceProductsAsync");
                SetStatus(ex.Message, StatusType.Error);
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "FetchSupplierInvoiceProductsAsync");
                SetStatus(HandleException(ex, "فشل في تحميل منتجات الفاتورة"), StatusType.Error);
            }
            finally
            {
                IsBusy = false;
                LoadingMessage = "";
            }
        }

        private void FilterSupplierSuggestions(string input)
        {
            if (string.IsNullOrWhiteSpace(input) || input.Length < 2)
            {
                IsSupplierSuggestionOpen = false;
                FilteredSupplierSuggestions.Clear();
                return;
            }

            var matches = _allSuppliersCache
                .Where(name => name.Contains(input, StringComparison.OrdinalIgnoreCase))
                .OrderBy(name => name.Length)
                .Take(10)
                .ToList();

            FilteredSupplierSuggestions.Clear();
            foreach (var m in matches)
                FilteredSupplierSuggestions.Add(m);

            IsSupplierSuggestionOpen = FilteredSupplierSuggestions.Any();
        }

        private CancellationTokenSource _productSearchCts;
        private async Task SearchProductsAsync(string query)
        {
            // Cancel previous search
            _productSearchCts?.Cancel();
            _productSearchCts = new CancellationTokenSource();

            if (string.IsNullOrWhiteSpace(query) || query.Length < 2)
            {
                ProductSuggestions.Clear();
                return;
            }

            try
            {
                // Debounce - wait 300ms before searching
                await Task.Delay(300, _productSearchCts.Token);

                var result = await _inventoryService.GetAutocompleteProductsAsync(query);
                
                if (_productSearchCts.Token.IsCancellationRequested) return;

                ProductSuggestions.Clear();
                if (result != null)
                {
                    foreach (var item in result.Take(15))
                        ProductSuggestions.Add(item.Name);
                    
                    IsProductSuggestionOpen = ProductSuggestions.Any();
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when search is cancelled
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "SearchProductsAsync");
                // Silent fail
            }
        }

        private void AddSupplierItem()
        {
            // Validate current item
            if (string.IsNullOrWhiteSpace(CurrentSupplierItem.ProductName))
            {
                SetStatus("يرجى إدخال اسم المنتج", StatusType.Warning);
                return;
            }

            if (CurrentSupplierItem.Quantity <= 0)
            {
                SetStatus("يرجى إدخال كمية صحيحة (أكبر من صفر)", StatusType.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(CurrentSupplierItem.Reason))
            {
                SetStatus("يرجى إدخال سبب الإرجاع", StatusType.Warning);
                return;
            }

            // Check for duplicate product
            if (SupplierReturnItems.Any(i => i.ProductName.Equals(CurrentSupplierItem.ProductName, StringComparison.OrdinalIgnoreCase)))
            {
                SetStatus("هذا المنتج مضاف مسبقاً في القائمة", StatusType.Warning);
                return;
            }

            // Add to list
            SupplierReturnItems.Add(CurrentSupplierItem);
            
            // Reset for next item
            CurrentSupplierItem = new SupplierReturnItem();
            ProductSearchText = "";
            ClearStatus();

            OnPropertyChanged(nameof(CanGoNext));
            OnPropertyChanged(nameof(CanSubmit));
        }

        private void RemoveSupplierItem(SupplierReturnItem item)
        {
            if (item != null && SupplierReturnItems.Contains(item))
            {
                SupplierReturnItems.Remove(item);
                OnPropertyChanged(nameof(CanGoNext));
                OnPropertyChanged(nameof(CanSubmit));
            }
        }
        #endregion

        #region Submit Logic
        private async Task SubmitReturnAsync()
        {
            if (!ValidateAllSteps())
            {
                SetStatus("يرجى التأكد من إكمال جميع البيانات المطلوبة", StatusType.Warning);
                return;
            }

            // Show confirmation dialog
            var confirmMessage = IsCustomerReturn
                ? $"هل أنت متأكد من إرجاع {SelectedItemsCount} منتج(ات) للطلب رقم {OrderCode}؟"
                : $"هل أنت متأكد من إرجاع {SelectedSupplierInvoiceItemsCount} منتج(ات) من الفاتورة رقم {SupplierInvoiceCode} للمورد {SupplierName}؟";

            if (!ErrorHandlingService.Confirm(confirmMessage, "تأكيد عملية الإرجاع"))
            {
                return;
            }

            IsBusy = true;
            LoadingMessage = "جاري تنفيذ عملية الإرجاع...";
            ClearStatus();

            try
            {
                bool success;
                string errorMessage;

                if (IsCustomerReturn)
                {
                    (success, errorMessage) = await SubmitCustomerReturnAsync();
                }
                else
                {
                    (success, errorMessage) = await SubmitSupplierReturnAsync();
                }

                if (success)
                {
                    IsSuccess = true;
                    SetStatus("تمت عملية الإرجاع بنجاح! ✓", StatusType.Success);
                    ErrorHandlingService.ShowSuccess("تمت عملية الإرجاع بنجاح");
                }
                else
                {
                    SetStatus(errorMessage ?? "فشلت عملية الإرجاع", StatusType.Error);
                }
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "SubmitReturnAsync");
                SetStatus(HandleException(ex, "فشل في تنفيذ عملية الإرجاع"), StatusType.Error);
            }
            finally
            {
                IsBusy = false;
                LoadingMessage = "";
            }
        }

        private async Task<(bool Success, string ErrorMessage)> SubmitCustomerReturnAsync()
        {
            var selectedItems = OrderItems.Where(i => i.IsSelected).ToList();

            // Validate all items before submission
            foreach (var item in selectedItems)
            {
                if (item.ReturnQuantity <= 0)
                    return (false, $"الكمية غير صالحة للمنتج: {item.Dto.Productname}");
                
                if (item.ReturnQuantity > item.Dto.Quantity)
                    return (false, $"لا يمكن إرجاع كمية أكبر من المشتراة للمنتج: {item.Dto.Productname}");
                
                if (string.IsNullOrWhiteSpace(item.Reason))
                    return (false, $"يجب إدخال سبب الإرجاع للمنتج: {item.Dto.Productname}");
            }

            if (!int.TryParse(OrderCode, out int orderCodeInt))
                return (false, "رقم الطلب غير صالح");

            var request = new CreateReturnRequestDto
            {
                OrderCode = orderCodeInt,
                Items = selectedItems.Select(i => new CreateReturnItemDto
                {
                    ProductName = i.Dto.Productname,
                    Quantity = i.ReturnQuantity,
                    Reason = i.Reason
                }).ToList()
            };

            return await _returnsService.CreateReturnAsync(request);
        }

        private async Task<(bool Success, string ErrorMessage)> SubmitSupplierReturnAsync()
        {
            if (string.IsNullOrWhiteSpace(SupplierName))
                return (false, "اسم المورد مطلوب");

            // Get selected products from invoice
            var selectedInvoiceProducts = SupplierInvoiceProducts.Where(p => p.IsSelected).ToList();
            
            if (selectedInvoiceProducts.Count == 0)
                return (false, "يجب اختيار منتج واحد على الأقل من الفاتورة");

            // Validate all selected items
            foreach (var item in selectedInvoiceProducts)
            {
                if (item.ReturnQuantity <= 0)
                    return (false, $"الكمية غير صالحة للمنتج: {item.Dto.ProductName}");
                
                if (item.ReturnQuantity > item.Dto.Quantity)
                    return (false, $"لا يمكن إرجاع كمية أكبر من الموجودة في الفاتورة للمنتج: {item.Dto.ProductName}");
                
                if (string.IsNullOrWhiteSpace(item.Reason))
                    return (false, $"يجب إدخال سبب الإرجاع للمنتج: {item.Dto.ProductName}");
            }

            var request = new ReturnToSupplierRequestDto
            {
                SupplierName = SupplierName,
                Items = selectedInvoiceProducts.Select(i => new CreateReturnItemDto
                {
                    ProductName = i.Dto.ProductName,
                    Quantity = i.ReturnQuantity,
                    Reason = i.Reason
                }).ToList()
            };

            return await _returnsService.ReturnToSupplierAsync(request);
        }
        #endregion

        #region Reset & Helpers
        private void ResetAllSteps()
        {
            CurrentStep = 1;
            OrderCode = "";
            SupplierName = "";
            SupplierInvoiceCode = "";
            OrderItems.Clear();
            SupplierInvoiceProducts.Clear();
            SupplierReturnItems.Clear();
            CurrentSupplierItem = new SupplierReturnItem();
            ProductSearchText = "";
            ProductSuggestions.Clear();
            FilteredSupplierSuggestions.Clear();
            IsSuccess = false;
            ClearStatus();

            OnPropertyChanged(nameof(TotalItemsCount));
            OnPropertyChanged(nameof(SelectedItemsCount));
            OnPropertyChanged(nameof(TotalSupplierInvoiceItemsCount));
            OnPropertyChanged(nameof(SelectedSupplierInvoiceItemsCount));
        }

        private void StartNewReturn()
        {
            IsSuccess = false;
            ResetAllSteps();
        }

        private void ClearStatus()
        {
            StatusMessage = "";
            CurrentStatusType = StatusType.None;
        }

        private void SetStatus(string message, StatusType type)
        {
            StatusMessage = message;
            CurrentStatusType = type;
        }

        private string HandleException(Exception ex, string context)
        {
            // Log for developers
            ErrorHandlingService.LogError(ex, context);

            // Return Arabic user-friendly message
            return ex switch
            {
                ServiceException svc => svc.Message,
                System.Net.Http.HttpRequestException http => GetNetworkErrorMessage(http),
                TaskCanceledException => "انتهت مهلة الاتصال. يرجى المحاولة مرة أخرى.",
                OperationCanceledException => "تم إلغاء العملية.",
                _ => ErrorHandlingService.GetUserFriendlyMessage(ex, context)
            };
        }

        private string GetNetworkErrorMessage(System.Net.Http.HttpRequestException ex)
        {
            var msg = ex.Message?.ToLower() ?? "";
            
            if (msg.Contains("404") || msg.Contains("not found"))
                return "لم يتم العثور على البيانات المطلوبة";
            if (msg.Contains("401") || msg.Contains("unauthorized"))
                return "انتهت صلاحية الجلسة. يرجى تسجيل الدخول مرة أخرى.";
            if (msg.Contains("403") || msg.Contains("forbidden"))
                return "ليس لديك صلاحية لتنفيذ هذه العملية";
            if (msg.Contains("400") || msg.Contains("bad request"))
                return "البيانات المدخلة غير صحيحة";
            if (msg.Contains("500") || msg.Contains("internal server"))
                return "حدث خطأ في الخادم. يرجى المحاولة لاحقاً.";
            
            return "خطأ في الاتصال بالخادم. تحقق من اتصالك بالإنترنت.";
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        #endregion
    }

    #region Enums
    public enum ReturnType
    {
        Customer,
        Supplier
    }

    public enum StatusType
    {
        None,
        Success,
        Warning,
        Error,
        Info
    }
    #endregion

    #region Helper Classes
    /// <summary>
    /// Enhanced selectable order item with change notification
    /// </summary>
    public class SelectableOrderItemEnhanced : INotifyPropertyChanged
    {
        public OrderItemForReturnDto Dto { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        private int _returnQuantity;
        public int ReturnQuantity
        {
            get => _returnQuantity;
            set
            {
                // Clamp to valid range
                _returnQuantity = Math.Max(0, Math.Min(value, Dto.Quantity));
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsQuantityValid));
            }
        }

        private string _reason = "";
        public string Reason
        {
            get => _reason;
            set { _reason = value ?? ""; OnPropertyChanged(); OnPropertyChanged(nameof(HasReason)); }
        }

        public bool IsQuantityValid => ReturnQuantity > 0 && ReturnQuantity <= Dto.Quantity;
        public bool HasReason => !string.IsNullOrWhiteSpace(Reason);

        public SelectableOrderItemEnhanced(OrderItemForReturnDto dto)
        {
            Dto = dto ?? throw new ArgumentNullException(nameof(dto));
            ReturnQuantity = dto.Quantity; // Default to max quantity
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Item for supplier return with validation
    /// </summary>
    public class SupplierReturnItem : INotifyPropertyChanged
    {
        private string _productName = "";
        public string ProductName
        {
            get => _productName;
            set { _productName = value ?? ""; OnPropertyChanged(); }
        }

        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = Math.Max(0, value); OnPropertyChanged(); }
        }

        private string _reason = "";
        public string Reason
        {
            get => _reason;
            set { _reason = value ?? ""; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    /// <summary>
    /// Selectable wrapper for supplier invoice products
    /// </summary>
    /// <summary>
    /// Selectable wrapper for supplier invoice products
    /// </summary>
    public class SelectableSupplierInvoiceProduct : INotifyPropertyChanged
    {
        public SupplierInvoiceProductDto Dto { get; }

        public SelectableSupplierInvoiceProduct(SupplierInvoiceProductDto dto)
        {
            Dto = dto ?? throw new ArgumentNullException(nameof(dto));
            _returnQuantity = dto.Quantity; // Initialize field directly
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(); }
        }

        private int _returnQuantity;
        public int ReturnQuantity
        {
            get => _returnQuantity;
            set
            {
                // Strict validation: cannot be less than 0
                // We enforce the max quantity check here.
                int correctedValue = value;
                
                if (correctedValue > Dto.Quantity)
                {
                    correctedValue = Dto.Quantity;
                }
                else if (correctedValue < 0)
                {
                    correctedValue = 0;
                }

                if (_returnQuantity != correctedValue)
                {
                    _returnQuantity = correctedValue;
                    OnPropertyChanged();
                }
                else if (value != _returnQuantity)
                {
                    // If the user typed a wrong value but the corrected value is the same as current,
                    // we still notify to force the UI to revert to the valid value
                    OnPropertyChanged();
                }
            }
        }

        private string _reason = "";
        public string Reason
        {
            get => _reason;
            set { _reason = value ?? ""; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
    #endregion
}
