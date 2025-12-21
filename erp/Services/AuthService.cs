using System.Net;
using System.Threading;
using System.Threading.Tasks;
using erp.DTOs;

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
    public async Task<(HttpStatusCode StatusCode, LoginResult? Result)> LoginAsync(
        LoginRequest req, CancellationToken ct = default)
    {
        var (status, result) =
            await _api.PostWithStatusAsync<LoginResult>("api/Auth/login", req, ct);

        // ✅ نعتبر Login ناجح فقط لو:
        // 1) HTTP 200
        // 2) Success = true
        // 3) Token موجود
        if (status == HttpStatusCode.OK &&
            result?.Success == true &&
            !string.IsNullOrWhiteSpace(result.Auth?.Token))
        {
            _session.AccessToken = result.Auth!.Token;
            _session.RefreshToken = result.Auth.RefreshToken;
            _session.TokenExpiry = result.Auth.TokenExpiry;
        }

        return (status, result);
    }

    // =========================
    // POST /api/Auth/logout
    // =========================
    public async Task<ApiResponse<string>?> LogoutAsync(CancellationToken ct = default)
    {
        // لو مفيش Token → Already logged out
        if (string.IsNullOrWhiteSpace(_session.AccessToken))
        {
            return new ApiResponse<string>
            {
                Success = true,
                Message = "Already logged out."
            };
        }

        // ApiClient جديد بهيدر Authorization
        var authedHttp = ApiClient.CreateHttpClient(_session.AccessToken);
        var authedApi = new ApiClient(authedHttp);

        var (status, res) =
            await authedApi.PostWithStatusAsync<ApiResponse<string>>(
                "api/Auth/logout", new { }, ct);

        if (status == HttpStatusCode.OK && res?.Success == true)
            _session.Clear();

        return res;
    }
}
