using erp.DTOS.Inventory.Responses;
using erp.DTOS.OrderDTOs;
using erp.DTOS.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static erp.Services.InventoryService;

namespace erp.Services
{
    // ===================== Autocomplete DTOs =====================
    public class CustomerAutocompleteItem
    {
        public int usernumber { get; set; }
        public string fullname { get; set; } = "";
    }

    public class SalesRepAutocompleteItem
    {
        public int usernumber { get; set; }
        public string fullname { get; set; } = "";
    }

    public class ProductAutocompleteItem
    {
        public int code { get; set; }
        public string name { get; set; } = "";
    }

    public class AutocompleteResponse<T>
    {
        public int statusCode { get; set; }
        public string message { get; set; } = "";
        public string traceId { get; set; } = "";
        public List<T> value { get; set; } = new();
    }

    public class OrdersService
    {
        private readonly ApiClient _api;
        private const string BaseUrl = "http://warhouse.runasp.net"; // لو swagger موجود هنا

        public OrdersService(ApiClient api)
        {
            _api = api;
        }

        // ===================== Autocomplete APIs =====================

        /// <summary>
        /// البحث عن العملاء بالاسم (Autocomplete)
        /// </summary>
        public async Task<List<CustomerAutocompleteItem>> GetCustomersAutocompleteAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term)) return new();
                
                var res = await _api.GetAsync<AutocompleteResponse<CustomerAutocompleteItem>>(
                    $"{BaseUrl}/api/Orders/customers/autocomplete?term={Uri.EscapeDataString(term)}");

                return res?.value ?? new();
            }
            catch
            {
                return new();
            }
        }

        /// <summary>
        /// البحث عن المندوبين بالاسم (Autocomplete)
        /// </summary>
        public async Task<List<SalesRepAutocompleteItem>> GetSalesRepAutocompleteAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term)) return new();

                var res = await _api.GetAsync<AutocompleteResponse<SalesRepAutocompleteItem>>(
                    $"{BaseUrl}/api/Orders/SalesRep/autocomplete?term={Uri.EscapeDataString(term)}");

                return res?.value ?? new();
            }
            catch
            {
                return new();
            }
        }

        /// <summary>
        /// البحث عن المنتجات بالاسم (Autocomplete)
        /// </summary>
        public async Task<List<ProductAutocompleteItem>> GetProductsAutocompleteAsync(string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term)) return new();

                var res = await _api.GetAsync<AutocompleteResponse<ProductAutocompleteItem>>(
                    $"{BaseUrl}/api/Inventory/autocomplete?term={Uri.EscapeDataString(term)}");

                return res?.value ?? new();
            }
            catch
            {
                return new();
            }
        }

        // ===================== Orders Lists =====================

        // 🔹 الطلبات المؤكدة (من المندوبين)
        public async Task<List<OrderDto>> GetConfirmedOrdersAsync()
        {
            var res = await _api.GetAsync<
                ApiResponse<List<OrderDto>>
            >($"{BaseUrl}/api/Orders/GetAllConfirmedOrders");

            return res.value ?? new();
        }

        // 🔹 الطلبات المعتمدة
        public async Task<List<OrderDto>> GetApprovedOrdersAsync()
        {
            var res = await _api.GetAsync<
                ApiResponse<List<OrderDto>>
            >($"{BaseUrl}/api/Orders/GetAllApprovedOrders");

            return res.value ?? new();
        }

        // ===================== Single Order =====================

        // 🔹 جلب طلب واحد بالـ OrderId
        public async Task<OrderDto?> GetOrderByIdAsync(string orderId)
        {
            var orders = await GetApprovedOrdersAsync();

            return orders.FirstOrDefault(o => o.id == orderId);
        }

        // ===================== Order Items =====================

        public async Task<List<OrderItemDto>> GetOrderItemsAsync(string orderId)
        {
            try
            {
                var res = await _api.GetAsync<
                    ApiResponse<List<OrderItemDto>>
                >($"{BaseUrl}/api/Orders/GetOrderItemsByOrderId?orderId={orderId}");

                return res.value ?? new();
            }
            catch
            {
                return await GetOrderItemsByOrderIdAsync(orderId);
            }
        }

        public async Task<List<OrderItemDto>> GetOrderItemsByOrderIdAsync(string orderId)
        {
            var res = await _api.GetAsync<
                ApiResponse<List<OrderItemDto>>
            >($"{BaseUrl}/api/Returns/OrderItemsByOrderId?orderId={orderId}");

            return res.value ?? new();
        }

        /// <summary>
        /// Gets order items using the order CODE (integer).
        /// This is the correct endpoint as per API specification.
        /// Endpoint: GET /api/Returns/OrderItemsByOrderId?orderCode={orderCode}
        /// </summary>
        /// <param name="orderCode">The sequential order code (integer, e.g., 12)</param>
        public async Task<List<OrderItemDto>> GetOrderItemsByOrderCodeAsync(int orderCode)
        {
            var res = await _api.GetAsync<
                ApiResponse<List<OrderItemDto>>
            >($"{BaseUrl}/api/Returns/OrderItemsByOrderId?orderCode={orderCode}");

            return res.value ?? new();
        }


        // ===================== Actions =====================

        // 🔹 اعتماد طلب
        public async Task<bool> ApproveOrderAsync(string orderId)
        {
            await _api.PutAsync<object>(
                $"{BaseUrl}/api/Orders/ApproveOrderByStoreManager?orderId={orderId}",
                new { }
            );

            return true;
        }

        // 🔹 إنشاء طلب
        public async Task<bool> CreateOrderAsync(CreateOrderRequestDto request)
        {
            await _api.PostAsync<object>(
                $"{BaseUrl}/api/Orders/Create_Order_By_Store_Manager_By_Customer_Id_And_SalesRepId",
                request
            );
            return true;
        }

        public async Task<bool> CreateOrderAsync(
            CreateOrderRequestDto request,
            double commissionPercentage)
        {
            await _api.PostAsync<object>(
                $"{BaseUrl}/api/Orders/Create_Order_By_Store_Manager_By_Customer_Id_And_SalesRepId?CommissionPercentage={commissionPercentage}",
                request
            );
            return true;
        }

        // 🔹 جلب كل المنتجات للاستخدام في autocomplete
        public async Task<List<ProductLookupDto>> GetProductsLookupAsync()
        {
            var res = await _api.GetAsync<ApiResponse<List<ProductLookupDto>>>($"{BaseUrl}/api/Inventory/GetProductsLookup");
            return res.value ?? new List<ProductLookupDto>();
        }

    }
}
