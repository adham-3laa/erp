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

        #region Get Invoices List

        public async Task<List<InvoiceResponseDto>> GetInvoices(
            string search,
            string invoiceType,
            string query,
            DateTime? fromDate,
            DateTime? toDate,
            int page = 1,
            int pageSize = 10)
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(invoiceType))
                queryParams.Add($"invoiceType={invoiceType}");

            if (!string.IsNullOrWhiteSpace(query))
                queryParams.Add($"query={Uri.EscapeDataString(query)}");

            if (fromDate.HasValue)
                queryParams.Add($"fromDate={fromDate.Value:yyyy-MM-dd}");

            if (toDate.HasValue)
                queryParams.Add($"toDate={toDate.Value:yyyy-MM-dd}");

            queryParams.Add($"page={page}");
            queryParams.Add($"pageSize={pageSize}");

            var url = $"api/Invoices/list?{string.Join("&", queryParams)}";

             var response = await _client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<InvoiceResponseDto>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<InvoiceResponseDto>();
        }

        #endregion
    }
}
