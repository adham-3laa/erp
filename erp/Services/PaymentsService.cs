using erp.DTOS.InvoicesDTOS;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace erp.Services
{
    public class PaymentsService
    {
        private readonly HttpClient _client;

        public PaymentsService()
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

            if (!string.IsNullOrEmpty(TokenStore.Token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            }
        }

        // ================= Pay Supplier =================

        public async Task<PaySupplierResponseDto> PayToSupplier(
            Guid supplierInvoiceId,
            decimal paidAmount)
        {
            var body = new
            {
                supplierInvoiceId = supplierInvoiceId,
                payiedAmount = paidAmount
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json");

            var response = await _client.PostAsync(
                "api/Invoices/PayPartOfMoneyToSupplierBySupplierInvoiceId",
                content);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<PaySupplierResponseDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }
    }
}
