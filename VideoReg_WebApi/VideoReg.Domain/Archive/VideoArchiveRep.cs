using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using VideoReg.Domain.Archive.ArchiveFiles;
using VideoReg.Domain.Archive.BrigadeHistory;
using VideoReg.Domain.Archive.Config;
using VideoReg.Infra.Services;
using VideoReg.Infra.ValueTypes;

namespace VideoReg.Domain.Archive
{
    /// <summary>
    /// 
    /// </summary>
    public class VideoArchiveRep : IVideoArchiveRep
    {
        private readonly IMemoryCache cache;
        private readonly IVideoArchiveConfig config;
        private readonly IFileSystemService fs;
        readonly IArchiveFileGeneratorFactory fileGeneratorFactory;
        public VideoArchiveRep(IMemoryCache cache,
            IFileSystemService fs, 
            IVideoArchiveConfig config,
            IArchiveFileGeneratorFactory fileGeneratorFactory)
        {
            this.cache = cache;
            this.config = config;
            this.fs = fs;
            this.fileGeneratorFactory = fileGeneratorFactory;
        }

        public Stream GetVideoFileStream(DateTime pdt, int camera)
        {
            var fileFactory = fileGeneratorFactory.Create();
            var file = fileFactory.CreateVideoMp4(pdt, camera);
            var fullPath = Path.Combine(config.VideoArchivePath, file.fullArchiveName);
            if (!File.Exists(fullPath)) return default;
            var stream = fs.ReadFileToMemory(fullPath);
            return stream;
        }

        public FileVideoMp4[] GetStructure(DateTime startWith)
        {
            var res = cache.GetVideoArchiveCache();
            if (startWith == default)
                return res;
            var interval = new DateInterval(startWith, DateTime.MaxValue);
            return res.Where(x => interval.IsInInterval(x.pdt)).ToArray();
        }
    }
}
