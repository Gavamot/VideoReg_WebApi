using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using VideoReg.Domain.Config;
using VideoReg.Infra.Services;

namespace VideoReg.Domain.OnlineVideo
{
    public class FileTrendsRep : ITrendsRep
    {
        readonly ITrendsConfig config;
        private readonly IFileSystemService fileSystem;
        public FileTrendsRep(ITrendsConfig config, IFileSystemService fileSystem)
        {
            this.config = config;
            this.fileSystem = fileSystem;
        }

        public DateTime GetLastChanged()
        {
            return fileSystem.GetLastModification(config.TrendsFileName);;
        }

        public async Task<string> TryGetTrendsIfChangedAsync(DateTime timestamp)
        {
            DateTime lastMod = GetLastChanged();
            if (lastMod > timestamp)
            {
                return await fileSystem.ReadFileTextAsync(config.TrendsFileName, Encoding.UTF8);
            } 
            return null;
        }
    }
}
