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
                BaseAddress = new Uri("http://warhouse.runasp.net/")
            };

            if (!string.IsNullOrEmpty(TokenStore.Token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            }
        }

        // ================= Payed Amount From Customer / Supplier =================
        
        /// <summary>
        /// Pays an invoice by OrderId.
        /// 
        /// CRITICAL: The targetType parameter determines which invoice type is being paid:
        /// - "Customer" → CustomerInvoice
        /// - "SalesRep" → CommissionInvoice
        /// - "Return"   → ReturnInvoice (customer returns)
        /// 
        /// The backend API uses this to locate and update the correct invoice record.
        /// </summary>
        /// <param name="targetType">Customer, SalesRep, or Return</param>
        /// <param name="orderId">The Order GUID</param>
        /// <param name="paidAmount">Amount being paid</param>
        public async Task<PaidAmountResponseDto> PayedAmountFromCustomerByOrderID(
    string targetType,   // Customer | SalesRep | Return
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

            // Debug: Log the exact API call being made
            System.Diagnostics.Debug.WriteLine(
                $"[InvoicePaymentService] Payment request: PUT {url}");

            var response = await _client.PutAsync(url, null);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine(
                    $"[InvoicePaymentService] Payment FAILED: {response.StatusCode} - {error}");
                throw new Exception(error);
            }

            var json = await response.Content.ReadAsStringAsync();
            
            System.Diagnostics.Debug.WriteLine(
                $"[InvoicePaymentService] Payment SUCCESS: {json}");

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
