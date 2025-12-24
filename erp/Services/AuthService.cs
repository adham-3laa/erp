using System.Net;
using System.Threading;
using System.Threading.Tasks;
using erp.DTOS.Auth.Requests;
using erp.DTOS.Auth.Responses;

namespace erp.Services;

public sealed class AuthService
{
    private readonly ApiClient _api;
    private readonly IAuthSession _session;

    public AuthService(ApiClient api, IAuthSession session)
    {
        _api = api;
        _session = session;
    }

    // =========================
    // POST /api/Auth/login
    // =========================
    public async Task<(HttpStatusCode StatusCode, LoginResponse? Result)> LoginAsync(
        LoginRequest req, CancellationToken ct = default)
    {
        var (status, result) =
            await _api.PostWithStatusAsync<LoginResponse>("api/Auth/login", req, ct);

        // حفظ التوكن لو login كامل
        if (status == HttpStatusCode.OK &&
            result?.Success == true &&
            !string.IsNullOrWhiteSpace(result.Auth?.Token))
        {
            _session.AccessToken = result.Auth.Token;
            _session.RefreshToken = result.Auth.RefreshToken;
            _session.TokenExpiry = result.Auth.TokenExpiry;
        }

        return (status, result);
    }

    // =========================
    // POST /api/Auth/logout
    // =========================
    public async Task<ApiResultResponse?> LogoutAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            return new ApiResultResponse(true, "Already logged out.");
        }

        var authedHttp = ApiClient.CreateHttpClient(_session.AccessToken);
        var authedApi = new ApiClient(authedHttp);

        var (status, res) =
            await authedApi.PostWithStatusAsync<ApiResultResponse>(
                "api/Auth/logout", new { }, ct);

        if (status == HttpStatusCode.OK && res?.Success == true)
            _session.Clear();

        return res;
    }
}
