using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace DirectPackageInstaller.Others
{
    public static class HttpHelper
    {
        private static readonly SocketsHttpHandler Handler = new SocketsHttpHandler
        {
            PooledConnectionLifetime = TimeSpan.FromMinutes(15),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(2),
            MaxConnectionsPerServer = 100,
            AutomaticDecompression = DecompressionMethods.All,
            AllowAutoRedirect = true
        };

        public static readonly HttpClient Client = new HttpClient(Handler, disposeHandler: false)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        public static string UserAgent => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/137.0.0.0 Safari/537.36 DPI/" + SelfUpdate.CurrentVersion;

        public static async Task<string> DownloadStringAsync(string url, Cookie[]? cookies = null, Dictionary<string, string>? headers = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(UserAgent);

            if (headers != null)
            {
                foreach (var kvp in headers)
                    request.Headers.TryAddWithoutValidation(kvp.Key, kvp.Value);
            }

            if (cookies != null && cookies.Length > 0)
            {
                var cookieHeader = string.Join("; ", Array.ConvertAll(cookies, c => $"{c.Name}={c.Value}"));
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            }

            using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public static async Task<HttpResponseHeaders?> HeadAsync(string url, Cookie[]? cookies = null)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Head, url);
                request.Headers.UserAgent.ParseAdd(UserAgent);

                if (cookies != null && cookies.Length > 0)
                {
                    var cookieHeader = string.Join("; ", Array.ConvertAll(cookies, c => $"{c.Name}={c.Value}"));
                    request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
                }

                using var response = await Client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
                return response.Headers;
            }
            catch
            {
                return null;
            }
        }

        public static async Task<(byte[] Data, HttpResponseHeaders Headers)> DownloadDataAsync(string url, Cookie[]? cookies = null)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(UserAgent);

            if (cookies != null && cookies.Length > 0)
            {
                var cookieHeader = string.Join("; ", Array.ConvertAll(cookies, c => $"{c.Name}={c.Value}"));
                request.Headers.TryAddWithoutValidation("Cookie", cookieHeader);
            }

            using var response = await Client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var data = await response.Content.ReadAsByteArrayAsync();
            return (data, response.Headers);
        }
    }
}
