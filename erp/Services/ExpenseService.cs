using erp.DTOS;
using erp.DTOS.ExpensesDTOS;
using Refit;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace erp.Services
{
    public class ExpenseService
    {
        private readonly HttpClient _client;

        public ExpenseService()
        {
            var handler = new HttpClientHandler
            {
                // يتجاهل أي مشاكل في شهادة SSL
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };
            // استخدم TokenStore للحصول على التوكن الحالي
            _client = new HttpClient
            {
                BaseAddress = new Uri("https://be-positive.runasp.net/") // ضع هنا الـ API base URL الصحيح
            };

            if (!string.IsNullOrEmpty(TokenStore.Token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            }
        }

        #region Add Expense

        public async Task<ExpenseResponseDto> AddExpense(ExpenseCreateDto dto)
        {
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("api/Expenses", content);

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ExpenseResponseDto>(responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        #endregion

        #region Get All Expenses

        public async Task<List<ExpenseResponseDto>> GetAllExpenses()
        {
            var response = await _client.GetAsync("api/Expenses/GetAll");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<ExpenseResponseDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        #endregion

        #region Get My Expenses

        public async Task<List<ExpenseResponseDto>> GetMyExpenses()
        {
            var response = await _client.GetAsync("api/Expenses/myexpenses");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var value = doc.RootElement.GetProperty("value");

            return JsonSerializer.Deserialize<List<ExpenseResponseDto>>(
                value.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<ExpenseResponseDto>();
        }



        #endregion

        #region Get Expenses by Accountant

        public async Task<List<ExpenseResponseDto>> GetExpensesByAccountant()
        {
            var response = await _client.GetAsync("api/Expenses/ByAccountant");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<List<ExpenseResponseDto>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }


        #endregion
    }
}
