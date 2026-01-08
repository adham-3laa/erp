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
                BaseAddress = new Uri("http://localhost:5000/")
            };

            if (!string.IsNullOrEmpty(TokenStore.Token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            }
        }

        // ================= Payed Amount From Customer / Supplier =================
        public async Task<PaidAmountResponseDto> PayedAmountFromCustomerByOrderID(
    string targetType,   // Customer | SalesRep
    string orderId,
    decimal paidAmount)
        {
            if (paidAmount <= 0)
                throw new ArgumentException("Paid amount must be greater than zero");

            var url =
                $"api/Invoices/PayedAmountFromCustomerByOrderId" +
                $"?customerOrSalesRep={targetType}" +
                $"&orderId={orderId}" +
                $"&PayiedAmount={paidAmount}";

            var response = await _client.PutAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception(error);
            }

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<PaidAmountResponseDto>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            );
        }

    }
}
