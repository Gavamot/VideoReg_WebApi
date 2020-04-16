﻿using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WebApi.Core;
using WebApi.Core.SignalR;
using WebApi.OnlineVideo.OnlineVideo;
using WebApi.Services;

namespace WebApi.Archive
{
    public class InitHostedService : IHostedService
    {
        private readonly ILog log;
        private ITrendsArchiveRep trendsArchiveRep;
        private readonly ICameraArchiveRep videoArchiveRep;
        private readonly IClientAscHub clientHub;
        private readonly IArchiveTransmitter archiveTransmitter;
        public InitHostedService(ILog log,
            ITrendsArchiveRep trendsArchiveRep, 
            ICameraArchiveRep videoArchiveRep,
            IClientAscHub clientHub,
            IArchiveTransmitter archiveTransmitter)
        {
            this.log = log;
            this.trendsArchiveRep = trendsArchiveRep;
            this.videoArchiveRep = videoArchiveRep;
            this.archiveTransmitter = archiveTransmitter;
            this.clientHub = clientHub;
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

            clientHub.OnTrendsArchiveTask = async (pdt) => 
            {
                await archiveTransmitter.UploadTrendsFileAsync(pdt);
            };

            clientHub.OnCameraArchiveTask = async (pdt,camera) =>
            {
                await archiveTransmitter.UploadCameraFileAsync(pdt,camera);
            };

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
