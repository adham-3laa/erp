using erp.DTOS.Inventory.Responses;
using erp.DTOS.OrderDTOs;
using erp.DTOS.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace erp.Services
{
    public class OrdersService
    {
        private readonly ApiClient _api;

        public OrdersService(ApiClient api)
        {
            _api = api;
        }

        // ===================== Orders Lists =====================

        // 🔹 الطلبات المؤكدة (من المندوبين)
        public async Task<List<OrderDto>> GetConfirmedOrdersAsync()
        {
            var res = await _api.GetAsync<
                ApiResponse<List<OrderDto>>
            >("api/Orders/GetAllConfirmedOrders");

            return res.value ?? new();
        }

        // 🔹 الطلبات المعتمدة
        public async Task<List<OrderDto>> GetApprovedOrdersAsync()
        {
            var res = await _api.GetAsync<
                ApiResponse<List<OrderDto>>
            >("api/Orders/GetAllApprovedOrders");

            return res.value ?? new();
        }

        // ===================== Single Order =====================

        // 🔹 جلب طلب واحد بالـ OrderId
        public async Task<OrderDto?> GetOrderByIdAsync(string orderId)
        {
            // بما إن مفيش endpoint مباشر، بنجيب من المعتمدة
            var orders = await GetApprovedOrdersAsync();

            return orders.FirstOrDefault(o => o.id == orderId);
        }

        // ===================== Order Items =====================

        // 🔹 جلب بنود الطلب
        // ✅ تعديل: لو Endpoint بتاع Orders مش موجود/بيفشل -> يعمل fallback على Returns
        public async Task<List<OrderItemDto>> GetOrderItemsAsync(string orderId)
        {
            try
            {
                var res = await _api.GetAsync<
                    ApiResponse<List<OrderItemDto>>
                >($"api/Orders/GetOrderItemsByOrderId?orderId={orderId}");

                return res.value ?? new();
            }
            catch
            {
                // fallback على endpoint اللي انت متأكد إنه شغال
                return await GetOrderItemsByOrderIdAsync(orderId);
            }
        }

        public async Task<List<OrderItemDto>> GetOrderItemsByOrderIdAsync(string orderId)
        {
            var res = await _api.GetAsync<
                ApiResponse<List<OrderItemDto>>
            >($"api/Returns/OrderItemsByOrderId?orderId={orderId}");

            return res.value ?? new();
        }

        // ===================== Actions =====================

        // 🔹 اعتماد طلب
        public async Task<bool> ApproveOrderAsync(string orderId)
        {
            await _api.PutAsync<object>(
                $"api/Orders/ApproveOrderByStoreManager?orderId={orderId}",
                new { }   // body فاضي
            );

            return true;
        }

        // 🔹 إنشاء طلب
        public async Task<bool> CreateOrderAsync(CreateOrderRequestDto request)
        {
            await _api.PostAsync<object>(
                "api/Orders/Create_Order_By_Store_Manager_By_Customer_Id_And_SalesRepId",
                request
            );

            return true;
        }
    }
}
