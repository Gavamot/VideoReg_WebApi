using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WebApi.Core;
using WebApi.Services;

namespace WebApi.Archive
{
    public class InitHostedService : IHostedService
    {
        private readonly ILog log;
        private ITrendsArchiveRep trendsArchiveRep;
        private readonly ICameraArchiveRep videoArchiveRep;
      

        public InitHostedService(ILog log, ITrendsArchiveRep trendsArchiveRep, ICameraArchiveRep videoArchiveRep)
        {
            this.log = log;
            this.trendsArchiveRep = trendsArchiveRep;
            this.videoArchiveRep = videoArchiveRep;
        }

        private void TryStart<T>(T obj)
        {
            var rep = obj as IUpdatedCache;
            rep?.BeginUpdate();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TryStart(trendsArchiveRep);
            TryStart(videoArchiveRep);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
