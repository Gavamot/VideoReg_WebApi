using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using WebApi.CoreService;
using WebApi.Configuration;
using WebApi.Core.Archive;
using WebApi.Services;

namespace WebApi.Archive
{
    public class VideoArchiveUpdateHostedService : ServiceUpdater
    {
        private readonly IVideoArchiveConfig config;
        private readonly IVideoArchiveStructureStore cache;
        private readonly IVideoArchiveSource rep;
        public override string Name => "VideoArchiveUpdater";

        public VideoArchiveUpdateHostedService(ILog log, IVideoArchiveConfig config,
            IVideoArchiveStructureStore cache, IVideoArchiveSource rep) 
            : base(config.VideoArchiveUpdateTimeMs, log)
        {
            this.config = config;
            this.cache = cache;
            this.rep = rep;
        }

        public override Task DoWorkAsync(CancellationToken cancellationToken)
        {
            var files = rep.GetCompletedVideoFiles();
            cache.Set(files);
            return Task.CompletedTask;
        }
    }
}
