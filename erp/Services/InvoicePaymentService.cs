using erp.DTOS.InvoicesDTOS;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace erp.Services
{
    public class InvoicePaymentService
    {
        private readonly HttpClient _client;

        public InvoicePaymentService()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri("https://be-positive.runasp.net/")
            };

            if (!string.IsNullOrEmpty(TokenStore.Token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            }
        }

        // ================= Payed Amount From Customer / Supplier =================
        public async Task<PaidAmountResponseDto> PayedAmountFromCustomerByOrderID(
            string targetType,   // Customer | SalesRep | Supplier
            string orderId,
            decimal paidAmount)
        {
            var body = new
            {
                targetType,   // عميل / مندوب / مورد
                orderId,
                paidAmount
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _client.PostAsync(
                "api/Invoices/PayedAmountFromCustomerByOrderID",
                content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            return new PaidAmountResponseDto
            {
                PaidAmount = root.GetProperty("paidamount").GetDecimal(),
                RemainingAmount = root.GetProperty("remainingamount").GetDecimal(),
                CustomerName = root.TryGetProperty("customername", out var c)
                                ? c.GetString()
                                : null,
                SupplierName = root.TryGetProperty("suppliername", out var s)
                                ? s.GetString()
                                : null,
                SalesRepName = root.TryGetProperty("salesrepname", out var r)
                                ? r.GetString()
                                : null
            };
        }
    }
}
