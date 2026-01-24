using erp.DTOS;
using erp.DTOS.ExpensesDTOS;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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
                ServerCertificateCustomValidationCallback =
                    HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            };

            _client = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:7266/")
            };
        }

        private void AddAuthorizationHeader()
        {
            _client.DefaultRequestHeaders.Authorization = null;

            if (!string.IsNullOrEmpty(TokenStore.Token))
            {
                _client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            }
        }

        public async Task<ExpenseResponseDto> AddExpense(ExpenseCreateDto dto)
        {
            AddAuthorizationHeader();

            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("api/Expenses", content);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("جلسة العمل انتهت، يرجى تسجيل الدخول مرة أخرى");

            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();

            return JsonSerializer.Deserialize<ExpenseResponseDto>(
                responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? throw new Exception("فشل تحويل البيانات من السيرفر");
        }

        public async Task<List<ExpenseResponseDto>> GetAllExpenses()
        {
            AddAuthorizationHeader();

            var response = await _client.GetAsync("api/Expenses/GetAll");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("جلسة العمل انتهت، يرجى تسجيل الدخول مرة أخرى");

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var value = doc.RootElement.GetProperty("value");

            return JsonSerializer.Deserialize<List<ExpenseResponseDto>>(
                value.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<ExpenseResponseDto>();
        }

        public async Task<List<ExpenseResponseDto>> GetMyExpenses()
        {
            AddAuthorizationHeader();

            var response = await _client.GetAsync("api/Expenses/myexpenses");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("جلسة العمل انتهت، يرجى تسجيل الدخول مرة أخرى");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            JsonElement valueElement;

            if (doc.RootElement.TryGetProperty("value", out valueElement))
            {
                return JsonSerializer.Deserialize<List<ExpenseResponseDto>>(
                    valueElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<ExpenseResponseDto>();
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<ExpenseResponseDto>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<ExpenseResponseDto>();
            }
            else
            {
                return new List<ExpenseResponseDto>();
            }
        }

        // ✅ التعديل الأساسي: تمرير accountantUserId
        public async Task<List<ExpenseResponseDto>> GetExpensesByAccountant(string accountantUserId)
        {
            AddAuthorizationHeader();

            var response = await _client.GetAsync($"api/Expenses/ByAccountant?accountantUserId={accountantUserId}");

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                throw new Exception("جلسة العمل انتهت، يرجى تسجيل الدخول مرة أخرى");

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            JsonElement valueElement;

            if (doc.RootElement.TryGetProperty("value", out valueElement))
            {
                return JsonSerializer.Deserialize<List<ExpenseResponseDto>>(
                    valueElement.GetRawText(),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<ExpenseResponseDto>();
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                return JsonSerializer.Deserialize<List<ExpenseResponseDto>>(
                    json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                ) ?? new List<ExpenseResponseDto>();
            }
            else
            {
                return new List<ExpenseResponseDto>();
            }


        }

        // ✅ ميثود جديدة لجلب كل المستخدمين
        public async Task<List<UserDto>> GetAllUsers()
        {
            AddAuthorizationHeader();

            var response = await _client.GetAsync("api/Users/GetAll"); // تأكد إن هذا الـ endpoint موجود
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var value = doc.RootElement.GetProperty("value");

            return JsonSerializer.Deserialize<List<UserDto>>(
                value.GetRawText(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new List<UserDto>();
        }
    }
}
