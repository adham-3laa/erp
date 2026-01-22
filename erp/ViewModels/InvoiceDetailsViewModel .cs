using erp.DTOS.InvoicesDTOS;
using erp.DTOS.OrderDTOs;
using erp.Enums;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static erp.Services.InventoryService;


namespace erp.ViewModels.Invoices
{
    /// <summary>
    /// ViewModel for the Invoice Details Page.
    /// 
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// CRITICAL DESIGN RULE - ORDER-CENTRIC INVOICE DETAILS:
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// 
    /// Invoice Details = Order Details (for all invoice types EXCEPT SupplierInvoice)
    /// 
    /// ┌─────────────────────┬──────────────────────────────────────────────────────┐
    /// │ Invoice Type        │ Items Loading Strategy                               │
    /// ├─────────────────────┼──────────────────────────────────────────────────────┤
    /// │ CustomerInvoice     │ OrderId → GetOrderItemsByOrderId                     │
    /// │ CommissionInvoice   │ OrderId → GetOrderItemsByOrderId (same sales order)  │
    /// │ ReturnInvoice       │ OrderId → GetOrderItemsByOrderId (returned items)    │
    /// │ SupplierInvoice     │ Items[] embedded in invoice (NO OrderId)             │
    /// └─────────────────────┴──────────────────────────────────────────────────────┘
    /// 
    /// IDENTIFIERS:
    /// • InvoiceId  → string (GUID) - unique invoice identifier
    /// • OrderId    → string (GUID) - links invoice to order (THE source of truth)
    /// • InvoiceCode → int (sequential) - human-readable, used ONLY for SupplierInvoice
    /// 
    /// ═══════════════════════════════════════════════════════════════════════════════
    /// </summary>
    public class InvoiceDetailsViewModel : INotifyPropertyChanged
    {
        private readonly OrdersService _ordersService;
        private readonly InventoryService _inventoryService;
        private readonly InvoiceService _invoiceService;

        /// <summary>
        /// The invoice being displayed.
        /// </summary>
        public InvoiceResponseDto Invoice { get; }

        /// <summary>
        /// Collection of order items to display in the DataGrid.
        /// </summary>
        public ObservableCollection<InvoiceOrderItemRow> OrderItems { get; }
            = new ObservableCollection<InvoiceOrderItemRow>();

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string? _errorMessage;
        public string? ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasError)); }
        }

        public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

        public InvoiceDetailsViewModel(InvoiceResponseDto invoice)
        {
            Invoice = invoice ?? throw new ArgumentNullException(nameof(invoice));

            _ordersService = new OrdersService(App.Api);
            _inventoryService = new InventoryService();
            _invoiceService = new InvoiceService();

            _ = LoadAsync();
        }


        /// <summary>
        /// Loads invoice details based on the invoice type.
        /// This method implements the core rule: Order-based vs Code-based loading.
        /// 
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// LOADING STRATEGIES BY INVOICE TYPE:
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 
        /// ┌───────────────────────┬────────────────────────────────────────────────────┐
        /// │ Invoice Type          │ Loading Method                                     │
        /// ├───────────────────────┼────────────────────────────────────────────────────┤
        /// │ CustomerInvoice       │ LoadOrderBasedInvoiceItemsAsync (uses OrderCode)   │
        /// │ CommissionInvoice     │ LoadOrderBasedInvoiceItemsAsync (uses OrderCode)   │
        /// │ ReturnInvoice         │ LoadOrderBasedInvoiceItemsAsync (uses OrderCode)   │
        /// │ SupplierInvoice       │ LoadSupplierInvoiceItemsAsync (uses InvoiceCode)   │
        /// │ SupplierReturnInvoice │ LoadSupplierReturnInvoiceItemsAsync (InvoiceCode)  │
        /// └───────────────────────┴────────────────────────────────────────────────────┘
        /// </summary>
        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                OrderItems.Clear();

                // ═══════════════════════════════════════════════════════════════
                // STEP 1: Determine loading strategy based on invoice type
                // ═══════════════════════════════════════════════════════════════
                var invoiceType = Invoice.InvoiceTypeParsed;

                // ═══════════════════════════════════════════════════════════════
                // STRATEGY A: SUPPLIER RETURN INVOICE (MANDATORY ENDPOINT)
                // Endpoint: GET /api/Returns/GetAllProductsInSpecificReturnSupplierInvoice
                // Parameter: invoiceCode ONLY
                // ⚠️ DO NOT use OrderId, CustomerId, or SalesRepId
                // ═══════════════════════════════════════════════════════════════
                if (invoiceType.IsSupplierReturn())
                {
                    await LoadSupplierReturnInvoiceItemsAsync();
                    return;
                }

                // ═══════════════════════════════════════════════════════════════
                // STRATEGY B: SUPPLIER INVOICE (uses InvoiceCode / embedded Items)
                // ═══════════════════════════════════════════════════════════════
                if (invoiceType == InvoiceType.SupplierInvoice)
                {
                    await LoadSupplierInvoiceItemsAsync();
                    return;
                }

                // ═══════════════════════════════════════════════════════════════
                // STRATEGY C: ORDER-BASED INVOICES (Customer, Commission, Return)
                // Single source of truth: OrderCode
                // ═══════════════════════════════════════════════════════════════
                if (invoiceType.UsesOrderId())
                {
                    await LoadOrderBasedInvoiceItemsAsync();
                    return;
                }

                // ═══════════════════════════════════════════════════════════════
                // FALLBACK: Unknown invoice type
                // ═══════════════════════════════════════════════════════════════
                ErrorMessage = $"نوع فاتورة غير معروف: {Invoice.Type}";
            }
            catch (Exception ex)
            {
                ErrorMessage = $"خطأ في تحميل تفاصيل الفاتورة: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }


        /// <summary>
        /// Refreshes the invoice header data (Amounts, Payments) from the API.
        /// Useful after returning from a payment page.
        /// </summary>
        public async Task RefreshInvoiceDataAsync()
        {
            if (Invoice == null) return;

            try
            {
                IsLoading = true;

                // Determine the correct filters to find this specific invoice again
                // Determine the correct filters to find this specific invoice again
                // We use the ToApiString() extension to get the correct API value for any invoice type
                string typeFilter = Invoice.InvoiceTypeParsed.ToApiString();
                
                // Fallback for Unknown types (shouldn't happen for valid invoices)
                if (string.IsNullOrEmpty(typeFilter)) typeFilter = null;

                // Use the code for search as it acts as a unique human-readable ID
                var searchCode = Invoice.code.ToString();

                var result = await _invoiceService.GetInvoices(
                    search: searchCode,
                    invoiceType: typeFilter,
                    query: null,
                    orderId: null,
                    lastInvoice: null,
                    fromDate: null,
                    toDate: null,
                    page: 1,
                    pageSize: 50
                );

                // Find the exact invoice by ID to be sure
                var updatedInvoice = result.Items?.FirstOrDefault(x => x.Id == Invoice.Id);

                if (updatedInvoice != null)
                {
                    // Update observable properties to reflect UI changes
                    Invoice.PaidAmount = updatedInvoice.PaidAmount;
                    Invoice.RemainingAmount = updatedInvoice.RemainingAmount;
                    Invoice.Amount = updatedInvoice.Amount;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"فشل تحديث بيانات الفاتورة: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Loads items for Supplier Invoice.
        /// Uses the API endpoint: GET /api/Invoices/GetSupplierInviceProductsByInvoicCode?invoiceCode={code}
        /// Fallback: embedded Items list if API call fails.
        /// </summary>

        private async Task LoadSupplierInvoiceItemsAsync()
        {
            // Validate invoice code exists
            if (Invoice.code <= 0)
            {
                // Fallback to embedded items if no code
                LoadSupplierInvoiceFromEmbeddedItems();
                return;
            }

            try
            {
                // Fetch from API using invoice code
                var products = await _invoiceService.GetSupplierInvoiceProductsAsync(Invoice.code);

                if (products == null || products.Count == 0)
                {
                    // Fallback to embedded items
                    LoadSupplierInvoiceFromEmbeddedItems();
                    return;
                }

                // Map API response to display rows
                foreach (var product in products)
                {
                    OrderItems.Add(new InvoiceOrderItemRow
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName ?? "",
                        CategoryName = "غير محدد", // Not provided by API
                        Quantity = product.Quantity,
                        UnitPrice = product.BuyPrice,
                        Total = product.TotalPrice
                    });
                }
            }
            catch
            {
                // Fallback to embedded items on error
                LoadSupplierInvoiceFromEmbeddedItems();
            }
        }

        /// <summary>
        /// Fallback: loads supplier invoice items from embedded Items list.
        /// </summary>
        private void LoadSupplierInvoiceFromEmbeddedItems()
        {
            if (Invoice.Items == null || Invoice.Items.Count == 0)
            {
                ErrorMessage = "لم يتم العثور على عناصر لهذه الفاتورة";
                return;
            }

            foreach (var item in Invoice.Items)
            {
                OrderItems.Add(new InvoiceOrderItemRow
                {
                    ProductName = item.ProductName ?? "",
                    CategoryName = item.CategoryName ?? "غير محدد",
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    Total = item.UnitPrice * item.Quantity
                });
            }
        }


        /// <summary>
        /// Loads items for Supplier Return Invoice.
        /// 
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// MANDATORY ENDPOINT (Single Source of Truth):
        /// GET /api/Returns/GetAllProductsInSpecificReturnSupplierInvoice?invoiceCode={code}
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 
        /// This method handles the specialized loading of Supplier Return Invoice details.
        /// 
        /// ⚠️ MANDATORY CONSTRAINTS:
        /// - Use invoiceCode ONLY (integer)
        /// - Do NOT use OrderId, CustomerId, or SalesRepId
        /// - Do NOT infer data from normal supplier invoices
        /// - Do NOT mix this logic with customer or sales return invoices
        /// 
        /// Each returned item represents:
        /// - Product returned to supplier
        /// - Quantity returned
        /// - Buy price at time of return
        /// - Total price (quantity × buyPrice)
        /// 
        /// This is an ERP-critical financial and inventory document.
        /// Returns affect both inventory counts and supplier accounting.
        /// </summary>
        private async Task LoadSupplierReturnInvoiceItemsAsync()
        {
            // ═══════════════════════════════════════════════════════════════
            // VALIDATION: InvoiceCode is MANDATORY
            // ═══════════════════════════════════════════════════════════════
            if (Invoice.code <= 0)
            {
                ErrorMessage = "فاتورة مرتجع المورد لا تحتوي على كود الفاتورة (InvoiceCode). لا يمكن تحميل العناصر.";
                return;
            }

            var invoiceCode = Invoice.code;

            try
            {
                // ═══════════════════════════════════════════════════════════════
                // CALL MANDATORY ENDPOINT
                // Endpoint: GET /api/Returns/GetAllProductsInSpecificReturnSupplierInvoice
                // Parameter: invoiceCode={invoiceCode}
                // ═══════════════════════════════════════════════════════════════
                var products = await _invoiceService.GetSupplierReturnInvoiceProductsAsync(invoiceCode);

                if (products == null || products.Count == 0)
                {
                    ErrorMessage = "لم يتم العثور على منتجات مرتجعة لهذه الفاتورة";
                    return;
                }

                // ═══════════════════════════════════════════════════════════════
                // MAP API RESPONSE TO DISPLAY ROWS
                // Render the returned product list exactly as received
                // ═══════════════════════════════════════════════════════════════
                foreach (var product in products)
                {
                    OrderItems.Add(new InvoiceOrderItemRow
                    {
                        ProductId = product.ProductId,
                        ProductName = product.ProductName ?? "",
                        CategoryName = "مرتجع مورد", // Mark as supplier return
                        Quantity = product.Quantity,
                        UnitPrice = product.BuyPrice,
                        Total = product.TotalPrice
                    });
                }

                // Debug: Log success
                System.Diagnostics.Debug.WriteLine(
                    $"[InvoiceDetailsVM] Loaded {products.Count} products for SupplierReturnInvoice code={invoiceCode}");
            }
            catch (Exception ex)
            {
                // No fallback - this is the ONLY source of truth
                ErrorMessage = $"فشل في جلب عناصر فاتورة مرتجع المورد: {ex.Message}";
                System.Diagnostics.Debug.WriteLine(
                    $"[InvoiceDetailsVM] ERROR loading SupplierReturnInvoice: {ex.Message}");
            }
        }


        /// <summary>
        /// Loads items for Order-based invoices (Customer, Commission, Return).
        /// CRITICAL: Uses OrderCode (integer) as required by the API endpoint.
        /// Endpoint: GET /api/Returns/OrderItemsByOrderId?orderCode={orderCode}
        /// </summary>
        private async Task LoadOrderBasedInvoiceItemsAsync()
        {
            // ═══════════════════════════════════════════════════════════════
            // VALIDATION: OrderCode must exist for order-based invoices
            // The API uses orderCode (int), not orderId (GUID)
            // ═══════════════════════════════════════════════════════════════
            if (!Invoice.OrderCode.HasValue || Invoice.OrderCode.Value <= 0)
            {
                // Fallback: Try using OrderId if OrderCode is not available
                if (Invoice.OrderId.HasValue && Invoice.OrderId.Value != Guid.Empty)
                {
                    await LoadOrderBasedInvoiceItemsByOrderIdFallbackAsync();
                    return;
                }

                ErrorMessage = GetMissingOrderCodeErrorMessage(Invoice.InvoiceTypeParsed);
                return;
            }

            var orderCode = Invoice.OrderCode.Value;

            // ═══════════════════════════════════════════════════════════════
            // FETCH ORDER ITEMS VIA OrderCode (the correct API endpoint)
            // ═══════════════════════════════════════════════════════════════
            List<OrderItemDto> items;

            try
            {
                // Primary endpoint: uses orderCode (integer)
                items = await _ordersService.GetOrderItemsByOrderCodeAsync(orderCode);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"فشل في جلب عناصر الطلب: {ex.Message}";
                return;
            }

            if (items == null || items.Count == 0)
            {
                ErrorMessage = "لم يتم العثور على عناصر للطلب المرتبط بهذه الفاتورة";
                return;
            }

            // Populate the grid
            await PopulateOrderItemsAsync(items);
        }

        /// <summary>
        /// Fallback method: tries to load items using OrderId (GUID) for backwards compatibility.
        /// </summary>
        private async Task LoadOrderBasedInvoiceItemsByOrderIdFallbackAsync()
        {
            var orderId = Invoice.OrderId!.Value.ToString();

            List<OrderItemDto> items;

            try
            {
                items = await _ordersService.GetOrderItemsByOrderIdAsync(orderId);
            }
            catch
            {
                try
                {
                    items = await _ordersService.GetOrderItemsAsync(orderId);
                }
                catch (Exception fallbackEx)
                {
                    ErrorMessage = $"فشل في جلب عناصر الطلب: {fallbackEx.Message}";
                    return;
                }
            }

            if (items == null || items.Count == 0)
            {
                ErrorMessage = "لم يتم العثور على عناصر للطلب المرتبط بهذه الفاتورة";
                return;
            }

            await PopulateOrderItemsAsync(items);
        }

        /// <summary>
        /// Populates the OrderItems collection with data from the API.
        /// </summary>
        private async Task PopulateOrderItemsAsync(List<OrderItemDto> items)
        {


            // ═══════════════════════════════════════════════════════════════
            // ENRICH WITH PRODUCT DETAILS (category, prices)
            // ═══════════════════════════════════════════════════════════════
            var productMap = await GetProductLookupMapAsync();

            foreach (var item in items)
            {
                var row = new InvoiceOrderItemRow
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    ProductName = item.ProductName ?? "",
                    UnitPrice = item.Price
                };

                // Enrich with product catalog data
                EnrichRowWithProductData(row, productMap);

                row.Total = row.UnitPrice * row.Quantity;
                OrderItems.Add(row);
            }
        }

        /// <summary>
        /// Creates a lookup map of products for enrichment.
        /// </summary>
        private async Task<Dictionary<string, ProductLookupDto>> GetProductLookupMapAsync()
        {
            try
            {
                var products = await _inventoryService.GetAllProductsLookupAsync();
                return products
                    .Where(p => !string.IsNullOrWhiteSpace(p.ProductId))
                    .GroupBy(p => p.ProductId, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            }
            catch
            {
                // Return empty map - items will still display, just without enrichment
                return new Dictionary<string, ProductLookupDto>(StringComparer.OrdinalIgnoreCase);
            }
        }

        /// <summary>
        /// Enriches an item row with data from the product catalog.
        /// </summary>
        private void EnrichRowWithProductData(
            InvoiceOrderItemRow row,
            Dictionary<string, ProductLookupDto> productMap)
        {
            if (string.IsNullOrWhiteSpace(row.ProductId))
            {
                row.CategoryName = "غير محدد";
                return;
            }

            if (productMap.TryGetValue(row.ProductId, out var product))
            {
                // Fill missing product name
                if (string.IsNullOrWhiteSpace(row.ProductName))
                    row.ProductName = product.ProductName ?? "";

                // Use catalog price if order price is missing/zero
                if (row.UnitPrice <= 0)
                    row.UnitPrice = product.SalePrice;

                row.CategoryName = product.CategoryName ?? "غير محدد";
            }
            else
            {
                row.CategoryName = "غير محدد";
            }
        }

        /// <summary>
        /// Returns appropriate error message for missing OrderId based on invoice type.
        /// </summary>
        private string GetMissingOrderIdErrorMessage(InvoiceType type)
        {
            return type switch
            {
                InvoiceType.CustomerInvoice =>
                    "فاتورة العميل لا تحتوي على رقم الطلب (OrderId). لا يمكن تحميل العناصر.",

                InvoiceType.CommissionInvoice =>
                    "فاتورة العمولة لا تحتوي على رقم الطلب (OrderId). لا يمكن تحميل العناصر.",

                InvoiceType.ReturnInvoice =>
                    "فاتورة المرتجع لا تحتوي على رقم الطلب (OrderId). لا يمكن تحميل العناصر.",

                _ =>
                    "الفاتورة لا تحتوي على رقم الطلب المطلوب."
            };
        }

        /// <summary>
        /// Returns appropriate error message for missing OrderCode based on invoice type.
        /// </summary>
        private string GetMissingOrderCodeErrorMessage(InvoiceType type)
        {
            return type switch
            {
                InvoiceType.CustomerInvoice =>
                    "فاتورة العميل لا تحتوي على كود الطلب (OrderCode). لا يمكن تحميل العناصر.",

                InvoiceType.CommissionInvoice =>
                    "فاتورة العمولة لا تحتوي على كود الطلب (OrderCode). لا يمكن تحميل العناصر.",

                InvoiceType.ReturnInvoice =>
                    "فاتورة المرتجع لا تحتوي على كود الطلب (OrderCode). لا يمكن تحميل العناصر.",

                _ =>
                    "الفاتورة لا تحتوي على كود الطلب المطلوب."
            };
        }


        // ═══════════════════════════════════════════════════════════════
        // ROW DTO FOR DISPLAY
        // ═══════════════════════════════════════════════════════════════
        public class InvoiceOrderItemRow
        {
            public string? ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public string CategoryName { get; set; } = "غير محدد";
            public decimal Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Total { get; set; }
        }

        // ═══════════════════════════════════════════════════════════════
        // INotifyPropertyChanged
        // ═══════════════════════════════════════════════════════════════
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
