using erp.DTOS;
using erp.DTOS.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
            try
            {
                if (string.IsNullOrWhiteSpace(orderId))
                    throw new ArgumentException("OrderId is required", nameof(orderId));

                orderId = orderId.Trim();

                // If input is number (code), try endpoint supporting orderCode
                if (int.TryParse(orderId, out int orderCode))
                {
                    try
                    {
                        var byCode = await _api.GetAsync<ApiResponseForReturn<List<OrderItemForReturnDto>>>(
                            $"api/Returns/OrderItemsByOrderId?orderCode={orderCode}"
                        );
                        return byCode?.Value ?? new List<OrderItemForReturnDto>();
                    }
                    catch (Exception)
                    {
                        // Fallback: fetch valid orders to find ID from Code
                        // This handles cases where backend only accepts ID but user entered Code
                        var approvedOrders = await _ordersService.GetApprovedOrdersAsync();
                        var order = approvedOrders.FirstOrDefault(o => o.code == orderCode);

                        if (order == null || string.IsNullOrWhiteSpace(order.id))
                            throw new ServiceException($"الطلب برقم {orderCode} غير موجود أو غير معتمد.");

                        orderId = order.id.Trim();
                    }
                }

                // Fetch by ID (GUID)
                var response = await _api.GetAsync<ApiResponseForReturn<List<OrderItemForReturnDto>>>(
                    $"api/Returns/OrderItemsByOrderId?orderId={orderId}"
                );

                return response?.Value ?? new List<OrderItemForReturnDto>();
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, $"ReturnsService.GetOrderItemsByOrderIdAsync - Input: {orderId}");
                if (ex is ServiceException) throw;
                
                // Customize message if 404
                if (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
                    throw new ServiceException($"الطلب رقم {orderId} غير موجود.");

                throw new ServiceException("فشل في تحميل عناصر الطلب.", ex);
            }
        }

        // ===================== GET SUPPLIER INVOICE PRODUCTS BY INVOICE CODE =====================
        public async Task<List<SupplierInvoiceProductDto>> GetSupplierInvoiceProductsAsync(int invoiceCode)
        {
            try
            {
                if (invoiceCode <= 0)
                    throw new ArgumentException("رقم الفاتورة غير صالح", nameof(invoiceCode));

                ErrorHandlingService.LogInfo($"GetSupplierInvoiceProductsAsync | InvoiceCode: {invoiceCode}");

                var response = await _api.GetAsync<ApiResponseForReturn<List<SupplierInvoiceProductDto>>>(
                    $"api/Invoices/GetSupplierInviceProductsByInvoicCode?invoiceCode={invoiceCode}"
                );

                return response?.Value ?? new List<SupplierInvoiceProductDto>();
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, $"ReturnsService.GetSupplierInvoiceProductsAsync - InvoiceCode: {invoiceCode}");
                
                if (ex is ServiceException) throw;
                
                // Customize message if 404
                if (ex.Message.Contains("404") || ex.Message.Contains("Not Found"))
                    throw new ServiceException($"الفاتورة رقم {invoiceCode} غير موجودة.");

                throw new ServiceException("فشل في تحميل منتجات الفاتورة.", ex);
            }
        }

        // ===================== CREATE RETURN REQUEST (CUSTOMER) =====================
        public async Task<(bool Success, string? ErrorMessage)> CreateReturnAsync(CreateReturnRequestDto request)
        {
            try
            {
                if (request == null) return (false, "البيانات غير صالحة");

                ErrorHandlingService.LogInfo($"CreateReturnAsync | OrderCode: {request.OrderCode}");

                var result = await _api.PostWithStatusAsync<ApiResponseForReturn<object>>("api/Returns", request);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    return (true, null);

                var msg = ErrorHandlingService.GetMessageForStatusCode(result.StatusCode);
                return (false, msg);
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "ReturnsService.CreateReturnAsync");
                return (false, ErrorHandlingService.GetUserFriendlyMessage(ex, "فشل في إنشاء طلب الإرجاع"));
            }
        }

        // ===================== RETURN TO SUPPLIER =====================
        public async Task<(bool Success, string? ErrorMessage)> ReturnToSupplierAsync(ReturnToSupplierRequestDto request)
        {
            try
            {
                if (request == null) return (false, "البيانات غير صالحة");
                if (string.IsNullOrWhiteSpace(request.SupplierName)) return (false, "اسم المورد مطلوب");
                if (request.Items == null || !request.Items.Any()) return (false, "يجب إضافة منتجات للإرجاع");

                ErrorHandlingService.LogInfo($"ReturnToSupplierAsync | Supplier: {request.SupplierName}");

                var result = await _api.PostWithStatusAsync<object>("api/Returns/returntosupplier", request);

                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                    return (true, null);

                var msg = ErrorHandlingService.GetMessageForStatusCode(result.StatusCode);
                return (false, msg);
            }
            catch (Exception ex)
            {
                ErrorHandlingService.LogError(ex, "ReturnsService.ReturnToSupplierAsync");
                return (false, ErrorHandlingService.GetUserFriendlyMessage(ex, "فشل في إرجاع المنتجات للمورد"));
            }
        }
    }
}
