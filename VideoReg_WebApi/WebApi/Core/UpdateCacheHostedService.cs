using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WebApi.Core;
using WebApi.Services;

namespace WebApi.Archive
{
    public class UpdateCacheHostedService : IHostedService
    {
        private readonly ILog log;
        private ITrendsArchiveRep trendsArchiveRep;
        private readonly IVideoArchiveRep videoArchiveRep;
        public UpdateCacheHostedService(ILog log, ITrendsArchiveRep trendsArchiveRep, IVideoArchiveRep videoArchiveRep)
        {
            this.log = log;
            this.trendsArchiveRep = trendsArchiveRep;
            this.videoArchiveRep = videoArchiveRep;
        }

        private void TryStartUpdate<T>(T obj)
        {
            var rep = obj as IUpdatedCache;
            rep?.BeginUpdate();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TryStartUpdate(trendsArchiveRep);
            //TryStartUpdate(videoArchiveRep);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
