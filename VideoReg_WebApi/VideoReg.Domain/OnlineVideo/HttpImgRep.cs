using System;
using System.IO;
using System.Net;
using System.Net.Http;
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

        public async Task<byte[]> GetImgAsync(string url, int timeoutMs)
        {
            var client = httpFactory.CreateClient(url);
            client.Timeout = TimeSpan.FromMilliseconds(timeoutMs);
            using var response = await client.GetAsync(url);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new HttpImgRepStatusCodeException(url, response.StatusCode);
            return await response.Content.ReadAsByteArrayAsync();
        }

        //public byte[] GetImg(string url, int timeoutMs)
        //{
        //    var request = WebRequest.Create(url);
        //    request.Timeout = timeoutMs;

        //    using var response = (HttpWebResponse)request.GetResponse();
        //    if (response.StatusCode != HttpStatusCode.OK)
        //        throw new HttpImgRepStatusCodeException(url, response.StatusCode);

        //    using var ms = new MemoryStream();
        //    using (var stream = response.GetResponseStream())
        //    {
        //        if (stream != null)
        //        {
        //            stream.CopyTo(ms);
        //            stream.Close();
        //        }
        //    }

        //    byte[] res = ms.ToArray();
        //    response.Close();
        //    return res;
        //}
    }
}
