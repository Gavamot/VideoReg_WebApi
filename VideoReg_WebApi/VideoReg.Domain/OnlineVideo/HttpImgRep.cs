using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoReg.Domain.Archive.Config;
using VideoReg.Infra.Services;

namespace VideoReg.Domain.OnlineVideo
{
    public class HttpImgRepNetworkException : Exception
    {
        public HttpImgRepNetworkException(string url, Exception e) : base($"got exception then for GET img from {url}", e)
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
       // private readonly IHttpClientFactory httpFactory;
        private readonly ICameraConfig config;
        private readonly ILog log;
        public HttpImgRep(ICameraConfig config, ILog log)
        {
           // this.httpFactory = httpFactory;
            this.config = config;
            this.log = log;
        }


        private const string UserName = "admin";
        private const string Password = "admin1336";
        

        private async Task<(HttpStatusCode, bool, byte[])> GetByBasicAuthorization(Uri url, CancellationToken token)
        {
            using var httpClient = new HttpClient();
            string namePwd = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{UserName}:{Password}"));
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
            digestAuthorization.Add(url, "Digest", new NetworkCredential(UserName, Password));
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
