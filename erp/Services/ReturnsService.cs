using erp.DTOS;
using erp.DTOS.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;

namespace erp.Services
{
    public class ReturnsService
    {
        private readonly ApiClient _api;
        private readonly OrdersService _ordersService;

        public ReturnsService(ApiClient api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _ordersService = new OrdersService(api);
        }

        // ===================== GET ORDER ITEMS BY ORDER ID =====================
        public async Task<List<OrderItemForReturnDto>> GetOrderItemsByOrderIdAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("OrderId is required", nameof(orderId));

            orderId = orderId.Trim();

            // إذا كان المدخل رقم (code)، جرب endpoint اللي بيدعم orderCode مباشرة (زي Swagger)
            if (int.TryParse(orderId, out int orderCode))
            {
                try
                {
                    var byCode = await _api.GetAsync<ApiResponseForReturn<List<OrderItemForReturnDto>>>(
                        $"api/Returns/OrderItemsByOrderId?orderCode={orderCode}"
                    );

                    return byCode?.Value ?? new List<OrderItemForReturnDto>();
                }
                catch (Exception ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
                {
                    // fallback: بعض البيئات ممكن متدعمش orderCode، ساعتها نحاول نحوله لـ id
                }

                // fallback: جلب جميع الطلبات المعتمدة للبحث عن الطلب بـ code ثم استخدام id
                var approvedOrders = await _ordersService.GetApprovedOrdersAsync();
                var order = approvedOrders.FirstOrDefault(o => o.code == orderCode);

                if (order == null || string.IsNullOrWhiteSpace(order.id))
                    throw new Exception($"الطلب برقم {orderCode} غير موجود. تأكد من إدخال رقم الطلب الصحيح.");

                orderId = order.id.Trim();
            }

            // البحث عن العناصر باستخدام orderId (GUID)
            try
            {
                var response = await _api.GetAsync<ApiResponseForReturn<List<OrderItemForReturnDto>>>(
                    $"api/Returns/OrderItemsByOrderId?orderId={orderId}"
                );

                return response?.Value ?? new List<OrderItemForReturnDto>();
            }
            catch (Exception ex) when (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
            {
                throw new Exception($"الطلب برقم {orderId} غير موجود. تأكد من إدخال رقم الطلب الصحيح.", ex);
            }
        }

        // ===================== CREATE RETURN REQUEST =====================
        public async Task<bool> CreateReturnAsync(CreateReturnRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Debug.WriteLine($"[ReturnsService] CreateReturn | OrderCode: {request.OrderCode}");

            try
            {
                var response = await _api.PostAsync<ApiResponseForReturn<object>>(
                    "api/Returns",
                    request
                );

                return response?.StatusCode == 200;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ReturnsService] CreateReturn failed: {ex}");
                return false;
            }
        }
    }
}
