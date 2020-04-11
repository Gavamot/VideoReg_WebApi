using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WebApi.Archive.ArchiveFiles;
using WebApi.Configuration;
using WebApi.Core.Archive;
using WebApi.Services;

namespace WebApi.Archive
{
    public class TrendsArchiveRep : ITrendsArchiveRep
    {
        private readonly ITrendsArchiveStructureStore cache;
        private readonly ITrendsArchiveConfig config;
        private readonly IFileSystemService fs;

        public TrendsArchiveRep(ITrendsArchiveStructureStore cache,
            IFileSystemService fs, 
            ITrendsArchiveConfig config)
        {
            this.config = config;
            this.fs = fs;
            this.cache = cache;
        }

        public async Task<byte[]> GetTrendFileAsync(DateTime pdt)
        {
            var file = cache.GetAll().FirstOrDefault(x => x.pdt == pdt);
            if (file == default)
                return null;
            string filePath = Path.Combine(config.TrendsArchivePath, file.fullArchiveName);
            var stream = await fs.ReadFileAsync(filePath);
            return stream;
        }

        public FileTrendsJson[] GetFullStructure(DateTime startWith)
        {
            var res = cache.GetAll();
            if (startWith == default)
                return res;
            return res.Where(x => x.pdt >= startWith).ToArray();
        }
    }
}
