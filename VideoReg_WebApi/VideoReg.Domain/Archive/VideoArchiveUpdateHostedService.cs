using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using VideoReg.Domain.Archive;
using VideoReg.Domain.Archive.Config;
using VideoReg.Infra.Services;

namespace VideoRegService.Core.Archive
{
    public class VideoArchiveUpdateHostedService : ServiceUpdater
    {
        private readonly IVideoArchiveConfig config;
        private readonly IMemoryCache cache;
        private readonly IVideoArchiveSource rep;

        public VideoArchiveUpdateHostedService(ILog log, IVideoArchiveConfig config,
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
