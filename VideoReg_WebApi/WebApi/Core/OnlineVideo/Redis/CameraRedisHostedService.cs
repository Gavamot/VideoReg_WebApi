using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WebApi.Configuration;
using WebApi.OnlineVideo.Store;
using WebApi.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApi.Core.OnlineVideo;

namespace WebApi.OnlineVideo
{
    public class CameraRedisHostedService : ServiceUpdater, IHostedService
    {
        private readonly ISnapshotRep imgRep;
        readonly ICameraStore cameraCache;
        private ICameraConfig config;

        public override object Context { get; protected set; }
        public override string Name => "CameraUpdate";

        public CameraRedisHostedService(ISnapshotRep imgRep,
            ICameraStore cameraCache,
            ICameraConfig config,
            ILogger<CameraRedisHostedService> log) : base(config.CameraUpdateIntervalMs, log)
        {
            this.imgRep = imgRep;
            this.cameraCache = cameraCache;
            this.config = config;
        }


        private DateTime[] timestamps = new DateTime[9];

        /// <summary>
        /// DoWorkAsync
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        public override async Task DoWorkAsync(object context, CancellationToken cancellationToken)
        {
            Measure.Invoke(() =>
            {
                var snapshots = imgRep.GetAll();
                for (int i = 0; i < snapshots.Length; i++)
                {
                    var snapshot = snapshots[i];
                    if (snapshot == null) continue;
                    if (snapshot.Timestamp == timestamps[i]) continue;
                    cameraCache.SetCamera(i + 1, snapshot.GetImage());
                    timestamps[i] = snapshot.Timestamp;
                }
            }, out var time);
            await SleepIfNeedMsAsync((long) time.TotalMilliseconds, cancellationToken);
        }

        public override Task<bool> BeforeStart(object context, CancellationToken cancellationToken) =>
            Task.FromResult(true);

    }
}
