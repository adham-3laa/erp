using erp.DTOS.Inventory.Responses;
using erp.DTOS.Orders;
using System;
using System.Collections.Generic;
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

        // 🔹 اعتماد طلب
        // ApiClient.PutAsync<T> لازم body → نبعت object فاضي
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
