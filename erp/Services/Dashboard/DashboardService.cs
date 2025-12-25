using erp.DTOS.Dashboard;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace erp.Services.Dashboard
{
    public sealed class DashboardService
    {
        private readonly HttpClient _http;

        public DashboardService(HttpClient http)
        {
            _http = http ?? throw new ArgumentNullException(nameof(http));
        }

        public async Task<DashboardStatsResponse> GetStatsAsync(CancellationToken ct = default)
        {
            using var req = new HttpRequestMessage(HttpMethod.Get, "/api/Dashboard/stats");
            using var res = await _http.SendAsync(req, ct);

            var json = await res.Content.ReadAsStringAsync(ct);

            if (!res.IsSuccessStatusCode)
                throw new HttpRequestException($"Dashboard stats failed: {(int)res.StatusCode} - {json}");

            var data = JsonSerializer.Deserialize<DashboardStatsResponse>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );

            return data ?? throw new InvalidOperationException("Empty dashboard response.");
        }
    }
}
