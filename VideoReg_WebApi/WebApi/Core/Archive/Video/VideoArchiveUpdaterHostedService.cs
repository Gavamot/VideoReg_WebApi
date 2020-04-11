using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using WebApi.CoreService;
using WebApi.Configuration;
using WebApi.Core.Archive;
using WebApi.Services;

namespace WebApi.Archive
{
    public class VideoArchiveUpdaterHostedService : ServiceUpdater, IHostedService
    {
        private readonly IVideoArchiveConfig config;
        private readonly IVideoArchiveStructureStore cache;
        private readonly IVideoArchiveSource source;

        public override object Context { get; protected set; }
        public override string Name => "VideoArchiveUpdater";

        public VideoArchiveUpdaterHostedService(ILog log, IVideoArchiveConfig config,
            IVideoArchiveStructureStore cache, IVideoArchiveSource source) 
            : base(config.VideoArchiveUpdateTimeMs, log)
        {
            this.config = config;
            this.cache = cache;
            this.source = source;
        }

        public override Task<bool> BeforeStart(object context, CancellationToken cancellationToken) => Task.FromResult(true);

        public override Task DoWorkAsync(object context, CancellationToken cancellationToken)
        {
            var files = source.GetCompletedFiles();
            cache.Set(files);
            return Task.CompletedTask;
        }
    }
}
