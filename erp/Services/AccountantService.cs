using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

public class AccountantService
{
    private readonly HttpClient _httpClient;

    public AccountantService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new System.Uri("https://your-api-base-url.com"); // عدّلها حسب الـ API
    }

    public async Task<List<AccountantDto>> GetAllAccountantsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<AccountantDto>>("/api/AccountantProfiles/GetAllAccountants");
    }

    public async Task<AccountantDto> GetCurrentAccountantAsync()
    {
        return await _httpClient.GetFromJsonAsync<AccountantDto>("/api/AccountantProfiles/GetCurrentAccountant");
    }

    public async Task<bool> AddAccountantAsync(AccountantPostDto dto)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/AccountantProfiles/AddAccountant", dto);
        return response.IsSuccessStatusCode;
    }
}
