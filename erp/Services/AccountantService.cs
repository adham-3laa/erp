using erp.Dtos;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace erp.Services;

public class AccountantService
{
    private readonly HttpClient _http;

    public AccountantService(HttpClient httpClient)
    {
        _http = httpClient;
    }

    public Task<List<AccountantDto>?> GetAllAccountantsAsync()
        => _http.GetFromJsonAsync<List<AccountantDto>>("/api/AccountantProfiles/GetAllAccountants");

    public Task<AccountantDto?> GetCurrentAccountantAsync()
        => _http.GetFromJsonAsync<AccountantDto>("/api/AccountantProfiles/GetCurrentAccountant");

    public async Task<bool> AddAccountantAsync(AccountantPostDto dto)
    {
        var res = await _http.PostAsJsonAsync("/api/AccountantProfiles/AddAccountant", dto);
        return res.IsSuccessStatusCode;
    }
}
