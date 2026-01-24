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
    private readonly IAuthSession? _session;

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    // session optional
    public ApiClient(HttpClient http, IAuthSession? session = null)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _session = session;

        if (_http.DefaultRequestHeaders.Accept.Count == 0)
        {
            _http.DefaultRequestHeaders.Accept.Clear();
            _http.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
        }
    }

    public static HttpClient CreateHttpClient(string baseUrl = "http://localhost:7266/")
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri(baseUrl, UriKind.Absolute),
            Timeout = TimeSpan.FromSeconds(30)
        };

        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        return http;
    }

    // =========================
    // Basic HTTP helpers
    // =========================

    public async Task<T> GetAsync<T>(string url, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
        await EnsureSuccess(res).ConfigureAwait(false);

        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return DeserializeOrThrow<T>(json);
    }

    public async Task<T> PostAsync<T>(string url, object body, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Post, url)
        {
            Content = ToJsonContent(body)
        };
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
        await EnsureSuccess(res).ConfigureAwait(false);

        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return DeserializeOrThrow<T>(json);
    }

    public async Task<T> PutAsync<T>(string url, object body, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Put, url)
        {
            Content = ToJsonContent(body)
        };
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
        await EnsureSuccess(res).ConfigureAwait(false);

        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return DeserializeOrThrow<T>(json);
    }

    public async Task DeleteAsync(string url, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Delete, url);
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
        await EnsureSuccess(res).ConfigureAwait(false);
    }

    // =========================
    // Optional: WithStatus
    // =========================

    public Task<(HttpStatusCode StatusCode, T? Body)> PostWithStatusAsync<T>(
        string url, object body, CancellationToken ct = default)
        => SendJsonWithStatusAsync<T>(HttpMethod.Post, url, body, ct);

    public Task<(HttpStatusCode StatusCode, T? Body)> PutWithStatusAsync<T>(
        string url, object body, CancellationToken ct = default)
        => SendJsonWithStatusAsync<T>(HttpMethod.Put, url, body, ct);

    public async Task<(HttpStatusCode StatusCode, T? Body)> GetWithStatusAsync<T>(
        string url, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, url);
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
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
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);

        if (!res.IsSuccessStatusCode)
            throw new HttpRequestException($"Request failed: {(int)res.StatusCode} {res.ReasonPhrase}\n{json}");

        return (res.StatusCode, DeserializeOrDefault<T>(json));
    }

    private void ApplyAuth(HttpRequestMessage req)
    {
        var token = TokenStore.Token; // ?? ?????? ?? ??? TokenStore ??????
        if (!string.IsNullOrWhiteSpace(token))
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }


    private static StringContent ToJsonContent(object body)
        => new(JsonSerializer.Serialize(body, _jsonOptions), Encoding.UTF8, "application/json");

    private static T DeserializeOrThrow<T>(string json)
        => JsonSerializer.Deserialize<T>(json, _jsonOptions)
           ?? throw new InvalidOperationException("Empty/invalid JSON response.");

    private static T? DeserializeOrDefault<T>(string json)
        => string.IsNullOrWhiteSpace(json)
            ? default
            : JsonSerializer.Deserialize<T>(json, _jsonOptions);

    private static async Task EnsureSuccess(HttpResponseMessage res)
    {
        if (res.IsSuccessStatusCode) return;

        var body = await res.Content.ReadAsStringAsync().ConfigureAwait(false);
        throw new HttpRequestException(
            $"Request failed: {(int)res.StatusCode} {res.ReasonPhrase}\n{body}");
    }
    public async Task<T> PatchAsync<T>(string url, object body, CancellationToken ct = default)
    {
        using var req = new HttpRequestMessage(new HttpMethod("PATCH"), url)
        {
            Content = ToJsonContent(body)
        };
        ApplyAuth(req);

        using var res = await _http.SendAsync(req, ct).ConfigureAwait(false);
        await EnsureSuccess(res).ConfigureAwait(false);

        var json = await res.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
        return DeserializeOrThrow<T>(json);
    }
    private void ApplyFallbackToken(HttpRequestMessage req)
    {
        // لو فيه Authorization already
        if (req.Headers.Authorization != null)
            return;

        // خد التوكن من HttpClient نفسه
        var auth = _http.DefaultRequestHeaders.Authorization;
        if (auth == null)
            return;

        req.Headers.Authorization = auth;
    }


}
