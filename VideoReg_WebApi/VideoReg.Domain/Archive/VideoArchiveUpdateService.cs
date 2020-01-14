using System.Threading;
using Microsoft.Extensions.Caching.Memory;
using VideoReg.Domain.Archive;
using VideoReg.Domain.Archive.Config;
using VideoReg.Infra.Services;

namespace VideoRegService.Core.Archive
{
    public interface IVideoArchiveUpdateService { }
    public class VideoArchiveUpdateService : ServiceUpdater, IVideoArchiveUpdateService
    {
        private readonly IVideoArchiveConfig config;
        private readonly IMemoryCache cache;
        private readonly IVideoArchiveSource rep;

        public VideoArchiveUpdateService(ILog log, IVideoArchiveConfig config,
            IMemoryCache cache, IVideoArchiveSource rep) 
            : base(config.VideoArchiveUpdateTimeMs, log)
        {
            this.config = config;
            this.cache = cache;
            this.rep = rep;
        }

        public override string Name => "VideoArchiveUpdater";

        public override void DoWork(CancellationToken cancellationToken)
        {
            var files = rep.GetCompletedVideoFiles(); 
            cache.SetVideoArchiveCache(files);
        }
    }
}
