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
                BaseAddress = new Uri("http://localhost:7266/")
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

        /// <summary>
        /// Pays a Supplier Invoice using the Invoice GUID.
        /// Used for regular SupplierInvoice payments.
        /// </summary>
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

        /// <summary>
        /// Pays a Supplier Invoice using the Invoice CODE (integer).
        /// 
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// USED FOR: Supplier Return Invoices
        /// Endpoint: PUT /api/Invoices/PayPartOfMoneyToSupplierBySupplierInvoiceId
        /// Parameter: SupplierInvoiceId = invoice CODE (e.g., "33"), not GUID
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 
        /// Response structure:
        /// {
        ///   "statusCode": 200,
        ///   "message": "Success",
        ///   "amount": 36000,
        ///   "paidamount": 2000,
        ///   "remainingamount": 34000,
        ///   "supplier": "طه انور طه"
        /// }
        /// </summary>
        /// <param name="invoiceCode">The invoice code (sequential integer)</param>
        /// <param name="paidAmount">Amount to pay</param>
        /// <returns>Response with updated payment amounts</returns>
        public async Task<SupplierPaymentResponseDto> PaySupplierInvoiceByCode(int invoiceCode, decimal paidAmount)
        {
            AttachToken();

            if (invoiceCode <= 0)
                throw new ArgumentException("Invoice code must be a positive integer");

            if (paidAmount <= 0)
                throw new ArgumentException("Paid amount must be greater than zero");

            var url =
                $"api/Invoices/PayPartOfMoneyToSupplierBySupplierInvoiceId" +
                $"?SupplierInvoiceId={invoiceCode}&PayiedAmount={paidAmount}";

            System.Diagnostics.Debug.WriteLine(
                $"[InvoiceService] Paying by code: PUT {url}");

            var response = await _client.PutAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(
                    $"[InvoiceService] Payment FAILED: {response.StatusCode} - {error}");
                throw new Exception(error);
            }

            var json = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine(
                $"[InvoiceService] Payment SUCCESS: {json}");

            return JsonSerializer.Deserialize<SupplierPaymentResponseDto>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new SupplierPaymentResponseDto();
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
            
            // ✅ Debug: Log raw JSON to understand API response structure
            System.Diagnostics.Debug.WriteLine($"[InvoiceService] Raw JSON: {json.Substring(0, Math.Min(500, json.Length))}...");

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<InvoiceResponseDto>>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            var result = apiResponse?.value ?? new List<InvoiceResponseDto>();
            
            // ✅ Debug: Log first invoice code to verify parsing
            if (result.Count > 0)
            {
                System.Diagnostics.Debug.WriteLine($"[InvoiceService] First invoice - code: {result[0].code}, Id: {result[0].Id}");
            }

            return result;
        }

        // =====================================================
        // ========= Supplier Invoice Products =================
        // =====================================================

        /// <summary>
        /// Gets supplier invoice products by invoice code.
        /// Endpoint: GET /api/Invoices/GetSupplierInviceProductsByInvoicCode?invoiceCode={code}
        /// </summary>
        /// <param name="invoiceCode">The invoice code (sequential integer)</param>
        public async Task<List<SupplierInvoiceProductDto>> GetSupplierInvoiceProductsAsync(int invoiceCode)
        {
            AttachToken();

            var url = $"api/Invoices/GetSupplierInviceProductsByInvoicCode?invoiceCode={invoiceCode}";

            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({(int)response.StatusCode}): {err}");
            }

            var json = await response.Content.ReadAsStringAsync();

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<SupplierInvoiceProductDto>>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.value ?? new List<SupplierInvoiceProductDto>();
        }

        // =====================================================
        // ========= Supplier Return Invoice Products ==========
        // =====================================================

        /// <summary>
        /// Gets Supplier Return Invoice products by invoice code.
        /// 
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// MANDATORY ENDPOINT (Single Source of Truth):
        /// GET /api/Returns/GetAllProductsInSpecificReturnSupplierInvoice?invoiceCode={code}
        /// ═══════════════════════════════════════════════════════════════════════════════
        /// 
        /// This is the ONLY way to retrieve Supplier Return Invoice details.
        /// 
        /// ⚠️ CONSTRAINTS:
        /// - Use invoiceCode ONLY (do NOT use OrderId, CustomerId, or SalesRepId)
        /// - This is an ERP-critical financial and inventory document
        /// - The returned data affects supplier reconciliation and inventory counts
        /// </summary>
        /// <param name="invoiceCode">The invoice code (sequential integer) - REQUIRED</param>
        /// <returns>List of products returned to the supplier</returns>
        public async Task<List<SupplierReturnInvoiceProductDto>> GetSupplierReturnInvoiceProductsAsync(int invoiceCode)
        {
            if (invoiceCode <= 0)
                throw new ArgumentException("Invoice code must be a positive integer", nameof(invoiceCode));

            AttachToken();

            // ═══════════════════════════════════════════════════════════════
            // MANDATORY ENDPOINT - DO NOT MODIFY
            // ═══════════════════════════════════════════════════════════════
            var url = $"api/Returns/GetAllProductsInSpecificReturnSupplierInvoice?invoiceCode={invoiceCode}";

            var response = await _client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({(int)response.StatusCode}): {err}");
            }

            var json = await response.Content.ReadAsStringAsync();

            // Debug log for troubleshooting
            System.Diagnostics.Debug.WriteLine($"[InvoiceService] SupplierReturnInvoice products response: {json.Substring(0, Math.Min(500, json.Length))}...");

            var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<SupplierReturnInvoiceProductDto>>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return apiResponse?.value ?? new List<SupplierReturnInvoiceProductDto>();
        }
    }
}


