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
    public class TrendsArchiveUpdaterHostedService : ServiceUpdater, IHostedService
    {
        private readonly ITrendsArchiveConfig config;
        private readonly ITrendsArchiveStructureStore cache;
        private readonly ITrendsArchiveSource source;

        public override object Context { get; protected set; }
        public override string Name => "VideoArchiveUpdater";

        public TrendsArchiveUpdaterHostedService(ILog log, ITrendsArchiveConfig config,
            ITrendsArchiveStructureStore cache, ITrendsArchiveSource source) 
            : base(config.TrendsArchiveUpdateTimeMs, log)
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
