using erp.DTOS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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

        // ===================== GET PENDING RETURNS =====================
        public async Task<List<PendingReturnDto>> GetPendingReturnsAsync()
        {
            Debug.WriteLine("[ReturnsService] GetPendingReturns");

            try
            {
                var response = await _api.GetAsync<ApiResponseForReturn<List<PendingReturnDto>>>(
                    "api/Returns/pending"
                );

                return response?.StatusCode == 200 && response.Value != null
                    ? response.Value
                    : new List<PendingReturnDto>();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ReturnsService] GetPendingReturns failed: {ex}");
                return new List<PendingReturnDto>();
            }
        }

        // ===================== APPROVE RETURN =====================
        public async Task<bool> ApproveReturnAsync(string returnId)
        {
            if (string.IsNullOrWhiteSpace(returnId))
                throw new ArgumentException("ReturnId is required", nameof(returnId));

            Debug.WriteLine($"[ReturnsService] ApproveReturn | ReturnId: {returnId}");

            try
            {
                var response = await _api.PutAsync<ApiResponseForReturn<object>>(
                    $"api/Returns/approve?id={returnId}",
                    new { }
                );

                return response?.StatusCode == 200;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ReturnsService] ApproveReturn failed: {ex}");
                return false;
            }
        }
    }
}
