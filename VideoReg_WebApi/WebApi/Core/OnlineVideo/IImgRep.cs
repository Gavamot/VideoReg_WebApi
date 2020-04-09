using System;
using System.Threading;
using System.Threading.Tasks;

namespace WebApi.OnlineVideo
{
    public interface IImgRep
    {
        ///// <exception cref="HttpImgRepNetworkException">Network error</exception>
        ///// <exception cref="HttpImgRepStatusCodeException">Status code not equal 200</exception>
        Task<byte[]> GetImgAsync(Uri url, int timeoutMs, CancellationToken token = default);
    }
}
