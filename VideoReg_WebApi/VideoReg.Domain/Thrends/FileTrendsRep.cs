using System.IO;
using System.Threading.Tasks;
using VideoReg.Domain.Config;

namespace VideoReg.Domain.OnlineVideo
{
    public class FileTrendsRep : ITrendsRep
    {
        readonly ITrendsConfig config;
        public FileTrendsRep(ITrendsConfig config)
        {
            this.config = config;
        }

        public async Task<byte[]> GetThrendsAsync()
        {
            return await File.ReadAllBytesAsync(config.TrendsFileName);
        }
    }
}
