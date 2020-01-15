using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using VideoReg.Domain.Archive.ArchiveFiles;
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

        public VideoArchiveRep(IMemoryCache cache, IFileSystemService fs, IVideoArchiveConfig config)
        {
            this.cache = cache;
            this.config = config;
            this.fs = fs;
        }

        public MemoryStream GetVideoFileStream(string fileName)
        {
            fileName = Path.Combine(config.VideoArchivePath, fileName);
            var res = fs.ReadFileToMemory(fileName);
            return res;
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
