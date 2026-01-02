using erp.DTOS.InvoicesDTOS;
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
            // ================= SSL (Dev only) =================
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://be-positive.runasp.net/")
            };

            // ================= Attach JWT Token =================
            if (!string.IsNullOrWhiteSpace(TokenStore.Token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            }
        }

        // ================= Get Invoices List =================
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
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            // ================= SAFE invoiceType =================
            if (!string.IsNullOrWhiteSpace(invoiceType))
            {
                var normalizedType = invoiceType.Trim();

                // القيم المسموح بها من Swagger
                if (normalizedType == "CustomerInvoice" ||
                    normalizedType == "CommissionInvoice" ||
                    normalizedType == "SupplierInvoice" ||
                    normalizedType == "ReturnInvoice")
                {
                    queryParams.Add($"invoiceType={Uri.EscapeDataString(normalizedType)}");
                }
            }

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

            // ================= Better Error Handling =================
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Error ({response.StatusCode}): {error}");
            }

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<InvoicesListResponseDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? new InvoicesListResponseDto
            {
                Items = new List<InvoiceResponseDto>()
            };
        }

        // =========================================================
        // ================= Pay Supplier Invoice ==================
        // =========================================================

        /// <summary>
        /// دفع مبلغ (جزئي أو كلي) لفاتورة مورد باستخدام SupplierInvoiceId
        /// </summary>
        public async Task PaySupplierInvoice(Guid supplierInvoiceId, decimal paidAmount)
        {
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

    }
}
