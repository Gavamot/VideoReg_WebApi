using System.Threading.Tasks;

namespace VideoReg.Domain.OnlineVideo
{
    public interface ITrendsRep
    {
        Task<byte[]> GetThrendsAsync();
    }
}
