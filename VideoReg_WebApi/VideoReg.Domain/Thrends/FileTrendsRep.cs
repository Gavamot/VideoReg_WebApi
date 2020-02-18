using System.IO;
using System.Text;
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

        public async Task<string> GetThrendsAsync()
        {
            return await File.ReadAllTextAsync(config.TrendsFileName, Encoding.UTF8);
        }
    }
}
