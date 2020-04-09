using System;
using System.Text;
using System.Threading.Tasks;
using WebApi.Collection;
using WebApi.Configuration;
using WebApi.Services;

namespace WebApi.Trends
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

        public async Task<TimestampValue<string>> GetTrendsIfChangedAsync(DateTime timestamp)
        {
            DateTime lastMod = GetLastChanged();
            if (lastMod > timestamp)
            {
                var f = await fileSystem.ReadFileTextAsync(config.TrendsFileName, Encoding.UTF8);
                return new TimestampValue<string>(lastMod, f);
            } 
            return null;
        }
    }
}
