using erp.DTOS.Dashboard;


namespace erp.Services
{
    public sealed class DashboardService
    {
        private readonly ApiClient _api;

        public DashboardService(ApiClient api)
        {
            _api = api ?? throw new ArgumentNullException(nameof(api));
        }

        public Task<DashboardStatsResponse> GetStatsAsync(CancellationToken ct = default)
            => _api.GetAsync<DashboardStatsResponse>("/api/Dashboard/stats", ct);
    }
}
