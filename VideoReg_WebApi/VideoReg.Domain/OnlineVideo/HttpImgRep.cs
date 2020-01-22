using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        private readonly IHttpClientFactory httpFactory;
        private readonly ILog log;
        public HttpImgRep(IHttpClientFactory httpFactory, ILog log)
        {
            this.httpFactory = httpFactory;
            this.log = log;
        }

        public async Task<byte[]> GetImgAsync(Uri url, int timeoutMs, CancellationToken token)
        {
            var urlName = url.ToString();
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var client = httpFactory.CreateClient(urlName);
            var response = await client.SendAsync(request, token);
            if (response.IsSuccessStatusCode) 
                return await response.Content.ReadAsByteArrayAsync();
            throw new HttpImgRepStatusCodeException($"can not get image by address {urlName} bad status code={response.StatusCode}", response.StatusCode);
        }

        public byte[] GetImg(Uri url, int timeoutMs)
        {
            var request = WebRequest.Create(url);
            request.Timeout = timeoutMs;

            using var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpImgRepStatusCodeException(url.ToString(), response.StatusCode);

            using var ms = new MemoryStream();
            using (var stream = response.GetResponseStream())
            {
                if (stream != null)
                {
                    stream.CopyTo(ms);
                    stream.Close();
                }
            }

            byte[] res = ms.ToArray();
            response.Close();
            return res;
        }
    }
}
