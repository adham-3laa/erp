using erp.DTOS.InvoicesDTOS;
using erp.DTOS.OrderDTOs;
using erp.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace erp.ViewModels.Invoices
{
    public class InvoiceDetailsViewModel : INotifyPropertyChanged
    {
        private readonly OrdersService _ordersService;
        private readonly InventoryService _inventoryService;

        public InvoiceResponseDto Invoice { get; }

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
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public InvoiceDetailsViewModel(InvoiceResponseDto invoice)
        {
            Invoice = invoice;

            _ordersService = new OrdersService(App.Api);
            _inventoryService = new InventoryService();

            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = null;
                OrderItems.Clear();

                if (Invoice?.OrderId == null)
                    return;

                var orderId = Invoice.OrderId.Value.ToString();

                // ✅ 1) هات بنود الطلب
                List<OrderItemDto> items;

                try
                {
                    items = await _ordersService.GetOrderItemsByOrderIdAsync(orderId);
                }
                catch
                {
                    items = await _ordersService.GetOrderItemsAsync(orderId);
                }

                if (items == null || items.Count == 0)
                    return;

                // ✅ 2) هات Lookup للمنتجات (اسم + سعر + اسم فئة)
                var products = await _inventoryService.GetAllProductsLookupAsync();
                var productMap = products
                    .Where(p => !string.IsNullOrWhiteSpace(p.ProductId))
                    .GroupBy(p => p.ProductId, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

                // ✅ 3) كوّن Rows للعرض
                foreach (var it in items)
                {
                    var row = new InvoiceOrderItemRow
                    {
                        ProductId = it.ProductId,

                        // ✅ FIX: Quantity عندك decimal غالباً
                        Quantity = it.Quantity
                    };

                    // لو الـ API رجّع الاسم/السعر استخدمهم
                    row.ProductName = string.IsNullOrWhiteSpace(it.ProductName) ? "" : it.ProductName;

                    // ✅ safe conversion
                    row.UnitPrice = Convert.ToDecimal(it.Price);

                    // لو ناقصين، كمّلهم من الـ Inventory
                    if (!string.IsNullOrWhiteSpace(it.ProductId) && productMap.TryGetValue(it.ProductId, out var p))
                    {
                        if (string.IsNullOrWhiteSpace(row.ProductName))
                            row.ProductName = p.ProductName;

                        if (row.UnitPrice <= 0)
                            row.UnitPrice = p.SalePrice;

                        row.CategoryName = string.IsNullOrWhiteSpace(p.CategoryName) ? "غير محدد" : p.CategoryName;
                    }
                    else
                    {
                        row.CategoryName = "غير محدد";
                    }

                    // احسب الإجمالي
                    row.Total = row.UnitPrice * row.Quantity;

                    OrderItems.Add(row);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ===================== Row DTO للعرض =====================
        public class InvoiceOrderItemRow
        {
            public string? ProductId { get; set; }
            public string ProductName { get; set; } = "";
            public string CategoryName { get; set; } = "غير محدد";

            // ✅ FIX: خليها decimal عشان ما يحصلش CS0266
            public decimal Quantity { get; set; }

            public decimal UnitPrice { get; set; }
            public decimal Total { get; set; }
        }

        // ================= INotify =================
        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
