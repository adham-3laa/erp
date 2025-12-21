using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace erp.Services;

public sealed class ApiClient
{
    private readonly HttpClient _http;

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ApiClient(HttpClient http)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));

        // Ensure Accept header exists once
        if (_http.DefaultRequestHeaders.Accept.Count == 0)
        {
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }

    // Factory: HttpClient واحد للتطبيق
    // NOTE: BaseAddress optional (default = be-positive) to match teammate behavior, but keep config external if possible.
    public static HttpClient CreateHttpClient(
        string? bearerToken = null,
        string baseUrl = "http://be-positive.runasp.net/")
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(30)
        };

        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        if (!string.IsNullOrWhiteSpace(bearerToken))
        {
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", bearerToken);
        }

        return http;
    }

    // =========================
    // HTTP helpers (existing)
    // =========================

    public async Task<T> GetAsync<T>(string url, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync(url, ct).ConfigureAwait(false);
        await EnsureSuccess(res).ConfigureAwait(false);

        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return DeserializeOrThrow<T>(json);
    }

    public async Task<T> PostAsync<T>(string url, object body, CancellationToken ct = default)
    {
        using var content = ToJsonContent(body);
        using var res = await _http.PostAsync(url, content, ct).ConfigureAwait(false);
        await EnsureSuccess(res).ConfigureAwait(false);

        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return DeserializeOrThrow<T>(json);
    }

    public async Task<T> PutAsync<T>(string url, object body, CancellationToken ct = default)
    {
        using var content = ToJsonContent(body);
        using var res = await _http.PutAsync(url, content, ct).ConfigureAwait(false);
        await EnsureSuccess(res).ConfigureAwait(false);

        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return DeserializeOrThrow<T>(json);
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        using var res = await _http.DeleteAsync(url, ct).ConfigureAwait(false);
        await EnsureSuccess(res).ConfigureAwait(false);
    }

    // =========================
    // NEW: WithStatus helpers
    // =========================

    public Task<(HttpStatusCode StatusCode, T? Body)> PostWithStatusAsync<T>(
        string url, object body, CancellationToken ct = default)
        => SendJsonWithStatusAsync<T>(HttpMethod.Post, url, body, ct);

    public async Task<(HttpStatusCode StatusCode, T? Body)> GetWithStatusAsync<T>(
        string url, CancellationToken ct = default)
    {
        using var res = await _http.GetAsync(url, ct).ConfigureAwait(false);
        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Request failed: {(int)res.StatusCode} {res.ReasonPhrase}\n{json}");

        return (res.StatusCode, DeserializeOrDefault<T>(json));
    }

    // =========================
    // Internals
    // =========================

    private async Task<(HttpStatusCode StatusCode, T? Body)> SendJsonWithStatusAsync<T>(
        HttpMethod method, string url, object body, CancellationToken ct)
    {
        using var req = new HttpRequestMessage(method, url)
        {
            Content = ToJsonContent(body)
        };

        using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Request failed: {(int)res.StatusCode} {res.ReasonPhrase}\n{json}");

        return (res.StatusCode, DeserializeOrDefault<T>(json));
    }

    private static StringContent ToJsonContent(object body)
        => new(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");

    private static T DeserializeOrThrow<T>(string json)
        => JsonSerializer.Deserialize<T>(json, _jsonOptions)
           ?? throw new InvalidOperationException("Empty/invalid JSON response.");

    private static T? DeserializeOrDefault<T>(string json)
        => string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, _jsonOptions);

    private static async Task EnsureSuccess(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode) return;

        var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
        throw new HttpRequestException(
            $"Request failed: {(int)res.StatusCode} {res.ReasonPhrase}\n{body}");
    }
}
