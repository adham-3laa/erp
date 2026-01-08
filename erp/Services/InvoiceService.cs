using erp.DTOS.InvoicesDTOS;
using erp.DTOS.Inventory.Responses; // ApiResponse<T> => value
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace erp.Services
{
    public class InvoiceService
    {
        private readonly HttpClient _client;

        public InvoiceService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:5000/")
            };

            AttachToken();
        }

        private void AttachToken()
        {
            if (!string.IsNullOrWhiteSpace(TokenStore.Token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            }
        }

        // =====================================================
        // ============ LIST (Admin / Management) ==============
        // =====================================================
        // دي تفضل زي ما هي علشان صفحة "إدارة الفواتير"
        public async Task<InvoicesListResponseDto> GetInvoices(
            string search,
            string invoiceType,
            string query,
            string orderId,
            bool? lastInvoice,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 10)
        {
            AttachToken();

            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(invoiceType))
                queryParams.Add($"invoiceType={Uri.EscapeDataString(invoiceType)}");

            if (!string.IsNullOrWhiteSpace(query))
                queryParams.Add($"recipientName={Uri.EscapeDataString(query)}");

            if (!string.IsNullOrWhiteSpace(orderId))
                queryParams.Add($"orderId={Uri.EscapeDataString(orderId)}");

            if (lastInvoice.HasValue)
                queryParams.Add($"lastInvoice={lastInvoice.Value.ToString().ToLower()}");

            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");

            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var url = $"api/Invoices/list?{string.Join("&", queryParams)}";

            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<InvoicesListResponseDto>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new InvoicesListResponseDto();
        }

        // =====================================================
        // ================= USER-SCOPED =======================
        // =====================================================

        public Task<List<InvoiceResponseDto>> GetInvoicesForCustomer(string customerId)
            => GetValueList($"api/Invoices/AllInvoicesForSpecificCustomerByCustomerId?customerId={Uri.EscapeDataString(customerId)}");

        public Task<List<InvoiceResponseDto>> GetInvoicesForSupplier(string supplierId)
            => GetValueList($"api/Invoices/AllInvoicesForSpecificSupplierBySupplierId?supplierId={Uri.EscapeDataString(supplierId)}");

        public Task<List<InvoiceResponseDto>> GetInvoicesForSalesRep(string salesRepId)
            => GetValueList($"api/Invoices/AllInvoicesForSpecificSalesRepBySalesRepId?salesRepId={Uri.EscapeDataString(salesRepId)}");

        // =====================================================
        // ============== Pay Supplier Invoice =================
        // =====================================================

        public async Task PaySupplierInvoice(Guid supplierInvoiceId, decimal paidAmount)
        {
            AttachToken();

            if (paidAmount <= 0)
                throw new ArgumentException("Paid amount must be greater than zero");

            var url =
                $"api/Invoices/PayPartOfMoneyToSupplierBySupplierInvoiceId" +
                $"?SupplierInvoiceId={supplierInvoiceId}&PayiedAmount={paidAmount}";

            var response = await _client.PutAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }
        }

        // =====================================================
        // ================= Helper ============================
        // =====================================================

        // الـ endpoints الخاصة بالمستخدمين بترجع ApiResponse فيها value
        private async Task<List<InvoiceResponseDto>> GetValueList(string url)
        {
            AttachToken();

            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({(int)response.StatusCode}): {err}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<InvoiceResponseDto>>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.value ?? new List<InvoiceResponseDto>();
        }
    }
}
