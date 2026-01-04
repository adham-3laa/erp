using erp.DTOS;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;

namespace erp.Services
{
    public class ReturnsService
    {
        private readonly ApiClient _api;

        public ReturnsService(ApiClient api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        // ===================== GET ORDER ITEMS BY ORDER ID =====================
        public async Task<List<OrderItemForReturnDto>> GetOrderItemsByOrderIdAsync(string orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
                throw new ArgumentException("OrderId is required", nameof(orderId));

            var response = await _api.GetAsync<ApiResponseForReturn<List<OrderItemForReturnDto>>>(
                $"api/Returns/OrderItemsByOrderId?orderId={orderId}"
            );

            return response?.Value ?? new List<OrderItemForReturnDto>();
        }

        // ===================== CREATE RETURN REQUEST =====================
        public async Task<bool> CreateReturnAsync(CreateReturnRequestDto request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            Debug.WriteLine($"[ReturnsService] CreateReturn | OrderId: {request.OrderId}");

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
