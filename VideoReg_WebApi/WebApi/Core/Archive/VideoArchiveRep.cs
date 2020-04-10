using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WebApi.Archive.ArchiveFiles;
using WebApi.Configuration;
using WebApi.Core.Archive;
using WebApi.Services;
using WebApi.ValueType;

namespace WebApi.Archive
{
    /// <summary>
    /// 
    /// </summary>
    public class VideoArchiveRep : IVideoArchiveRep
    {
        private readonly IVideoArchiveStructureStore cache;
        private readonly IVideoArchiveConfig config;
        private readonly IFileSystemService fs;
        readonly IArchiveFileGeneratorFactory fileGeneratorFactory;
        public VideoArchiveRep(IVideoArchiveStructureStore cache,
            IFileSystemService fs, 
            IVideoArchiveConfig config,
            IArchiveFileGeneratorFactory fileGeneratorFactory)
        {
            this.config = config;
            this.fs = fs;
            this.fileGeneratorFactory = fileGeneratorFactory;
            this.cache = cache;
        }

        public MemoryStream GetVideoFileStream(DateTime pdt, int camera)
        {
            var file = cache.GetAll().FirstOrDefault(x => x.cameraNumber == camera && x.pdt == pdt);
            if (file == default)
                throw new FileNotFoundException("File does not exist in store");
            string filePath = Path.Combine(config.VideoArchivePath, file.fullArchiveName);
            var stream = fs.ReadFileToMemory(filePath);
            return stream;
        }

        public async Task<byte[]> GetVideoFileStreamAsync(DateTime pdt, int camera)
        {
            var file = cache.GetAll().FirstOrDefault(x => x.cameraNumber == camera && x.pdt == pdt);
            if (file == default)
                throw new FileNotFoundException("File does not exist in store");
            string filePath = Path.Combine(config.VideoArchivePath, file.fullArchiveName);
            var stream = await fs.ReadFileAsync(filePath);
            return stream;
        }

        public FileVideoMp4[] GetFullStructure(DateTime startWith = default)
        {
            var res = cache.GetAll();
            if (startWith == default)
                return res;
            return res.Where(x => x.pdt >= startWith).ToArray();
        }

        public FileVideoMp4[] GetStructureByCameraNumber(int cameraNumber, DateTime startWith = default)
        {
            var res = cache.GetByCameraNumber(cameraNumber);
            if (startWith == default)
                return res;
            return res.Where(x => x.pdt >= startWith).ToArray();
        }
    }
}
