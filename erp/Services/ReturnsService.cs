using erp.DTOS;
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
            _api = api;
        }

        // ===================== GET ORDER ITEMS BY ORDER ID =====================
        public async Task<List<OrderItemForReturnDto>> GetOrderItemsByOrderIdAsync(string orderId)
        {
            Debug.WriteLine($"[ReturnsService] GetOrderItemsByOrderId: {orderId}");

            try
            {
                var response =
                    await _api.GetAsync<
                        erp.DTOS.Inventory.Responses.ApiResponse<List<OrderItemForReturnDto>>
                    >($"api/Returns/OrderItemsByOrderId?orderId={orderId}");

                return response != null && response.statusCode == 200 && response.value != null
                    ? response.value
                    : new List<OrderItemForReturnDto>();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[ReturnsService] GetOrderItems failed: {ex.Message}");
                return new List<OrderItemForReturnDto>();
            }
        }

        // ===================== CREATE RETURN REQUEST =====================
        public async Task<bool> CreateReturnAsync(CreateReturnRequestDto request)
        {
            Debug.WriteLine($"[ReturnsService] CreateReturn | OrderId: {request.OrderId}");

            try
            {
                var response =
                    await _api.PostAsync<
                        erp.DTOS.Inventory.Responses.ApiResponse<object>
                    >("api/Returns", request);

                return response != null && response.statusCode == 200;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[ReturnsService] CreateReturn failed: {ex.Message}");
                return false;
            }
        }

        // ===================== GET PENDING RETURNS =====================
        public async Task<List<PendingReturnDto>> GetPendingReturnsAsync()
        {
            Debug.WriteLine("[ReturnsService] GetPendingReturns");

            try
            {
                var response =
                    await _api.GetAsync<
                        erp.DTOS.Inventory.Responses.ApiResponse<List<PendingReturnDto>>
                    >("api/Returns/pending");

                return response != null && response.statusCode == 200 && response.value != null
                    ? response.value
                    : new List<PendingReturnDto>();
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[ReturnsService] GetPendingReturns failed: {ex.Message}");
                return new List<PendingReturnDto>();
            }
        }

        // ===================== APPROVE RETURN =====================
        public async Task<bool> ApproveReturnAsync(string returnId)
        {
            Debug.WriteLine($"[ReturnsService] ApproveReturn | ReturnId: {returnId}");

            try
            {
                var response =
                    await _api.PutAsync<
                        erp.DTOS.Inventory.Responses.ApiResponse<object>
                    >($"api/Returns/approve?id={returnId}", new { });

                return response != null && response.statusCode == 200;
            }
            catch (System.Exception ex)
            {
                Debug.WriteLine($"[ReturnsService] ApproveReturn failed: {ex.Message}");
                return false;
            }
        }
    }
}
