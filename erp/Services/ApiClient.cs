using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace erp.Services;

public static class ApiClient
{
    public static HttpClient Create()
    {
        var http = new HttpClient
        {
            BaseAddress = new Uri("http://be-positive.runasp.net/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        http.DefaultRequestHeaders.Accept.Clear();
        http.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        return http;
    }
}
