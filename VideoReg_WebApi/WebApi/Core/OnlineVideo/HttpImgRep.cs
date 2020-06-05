using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Configuration;
using WebApi.Services;

namespace WebApi.OnlineVideo
{
    public class HttpImgRepNetworkException : Exception
    {
        public HttpImgRepNetworkException(string url, Exception e) : base($"got exception then for GET Image from {url}", e)
        {
        }
    }

    public class HttpImgRepStatusCodeException : Exception
    {
        public HttpImgRepStatusCodeException(string url, HttpStatusCode statusCode) 
            : base($"Response[GET {url}] -> {(int)statusCode} but it must be 200")
        {

        }
    }

    public class HttpImgRep : IImgRep
    {
        private readonly ICameraConfig config;
        private readonly ILogger<HttpImgRep> log;
        private readonly IHttpClientFactory httpFactory;
        public HttpImgRep(ICameraConfig config, ILogger<HttpImgRep> log, IHttpClientFactory httpFactory)
        {
            this.config = config;
            this.log = log;
            this.httpFactory = httpFactory;
        }

        private async Task<(HttpStatusCode, bool, byte[])> GetByBasicAuthorization(Uri url, CancellationToken token)
        {
            var httpClient = httpFactory.CreateClient(url.AbsoluteUri);
            string namePwd = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{config.UserName}:{config.Password}"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", namePwd);
            using var response = await httpClient.GetAsync(url, token);
            if (response.IsSuccessStatusCode)
            {
                var img = await response.Content.ReadAsByteArrayAsync();
                return (response.StatusCode, true, img);
            }
            return (response.StatusCode, false, null);
        }

        private async Task<(HttpStatusCode, bool, byte[])> GetByDigestAuthorization(Uri url, CancellationToken token)
        {
            var digestAuthorization = new CredentialCache();
            digestAuthorization.Add(url, "Digest", new NetworkCredential(config.UserName, config.Password));
            using var httpHandler = new HttpClientHandler { Credentials = digestAuthorization };
            using var httpClient = new HttpClient(httpHandler);
            using var response = await httpClient.GetAsync(url, token);
            if (response.IsSuccessStatusCode)
            {
                var img = await response.Content.ReadAsByteArrayAsync();
                return (response.StatusCode, true, img);
            }
            return (response.StatusCode, false, null);
        }

        public async Task<byte[]> GetImgAsync(Uri url, int timeoutMs, CancellationToken token)
        {
            var (statusCode, status, img) = await GetByDigestAuthorization(url, token);
            if (status) return img;

            (statusCode, status, img) = await GetByBasicAuthorization(url, token);
            if (status) return img;

            throw new HttpImgRepStatusCodeException($"can not get image by address {url} bad status code={statusCode}", statusCode);
        }
    }
}
