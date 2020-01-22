using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using VideoReg.Domain.Archive.Config;
using VideoReg.Domain.Store;
using VideoReg.Infra.Services;
using VideoRegService;

namespace VideoReg.Domain.OnlineVideo
{
    public class CameraUpdateService : ServiceUpdater
    {
        readonly ICameraStore cameraCache;
        readonly ICameraSourceRep sourceRep;
        private ICameraConfig config;
        private static volatile CameraSourceSettings[] camerasSettings = new CameraSourceSettings[0];
        private readonly Con
        private readonly IServiceProvider di;
        public CameraUpdateService(IImgRep imgRep,
            ICameraStore cameraCache,
            ICameraSourceRep cameraSourceRep,
            ICameraConfig config,
            IServiceProvider di,
            ILog log) : base(config.CameraUpdateIntervalMs, log)
        {
           // this.imgRep = imgRep;
            this.cameraCache = cameraCache;
            this.sourceRep = cameraSourceRep;
            this.config = config;
            this.di = di;
            BeforeStart += StartCameraThreads;
        }

        public override string Name => "CameraUpdate";

        protected void StartCameraThreads()
        {
            async Task UpdateCamera(int cameraNumber, IHttpClientFactory clientFactory)
            {
                var settings = camerasSettings.FirstOrDefault(x => x.number == cameraNumber);
                if (settings.number == 0) return;
                //if (GetUriWithAntiCacheParameter(settings.snapshotUrl, out Uri url))
                if(Uri.TryCreate(settings.snapshotUrl, UriKind.RelativeOrAbsolute, out var url))
                {
                    try
                    {
                        var request = new HttpRequestMessage(HttpMethod.Get, url);
                        var client = clientFactory.CreateClient();
                        var response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            byte[] img = await response.Content.ReadAsByteArrayAsync();
                            cameraCache.SetCamera(cameraNumber, img);
                        }
                        else
                        {
                            Thread.Sleep(config.CameraUpdateSleepIfErrorTimeoutMs);
                            log.Error($"camera[{cameraNumber}] - server return bad status code ={response.StatusCode}");
                        }
                    }
                    catch(Exception e)
                    {
                        Thread.Sleep(config.CameraUpdateSleepIfErrorTimeoutMs);
                        log.Error($"camera[{cameraNumber}] - error then update ({settings.snapshotUrl}) - [{e.Message}]", e);
                    }
                }
                else
                {
                    log.Warning($"camera[{cameraNumber}] has bad uri format {settings.snapshotUrl}");
                    Thread.Sleep(config.CameraUpdateSleepIfErrorTimeoutMs);
                }
            }

            void UpdateSleep(long operationExecutedMs)
            {
                long sleepTime = config.CameraUpdateIntervalMs - operationExecutedMs;
                if (sleepTime > 0 && sleepTime <= config.CameraUpdateIntervalMs)
                {
                    Thread.Sleep((int)sleepTime);
                }
            }

            for (int i = 1; i <= 9; i++)
            {
                int camNum = i;
                var starter = new ThreadStart(async() =>
                {
                    var clientFactory = (IHttpClientFactory)di.GetService(typeof(IHttpClientFactory));
                    while (true)
                    {
                        var watcher = new Stopwatch();
                        watcher.Start();
                        await UpdateCamera(camNum, clientFactory);
                        watcher.Stop();
                        UpdateSleep(watcher.ElapsedMilliseconds);
                    }
                });
                var thread = new Thread(starter);
                thread.Priority = ThreadPriority.Highest;
                thread.Name = $"Camera updater - {camNum}";
                thread.Start();
            }
        }

        public override async void DoWork(CancellationToken cancellationToken)
        {
            try
            {
                camerasSettings = await sourceRep.GetAll();
            }
            catch (Exception e)
            {
                await Task.Delay(config.CameraUpdateSleepIfErrorTimeoutMs, cancellationToken);
                log.Error($"Cannot find Cameras Settings ({e.Message})");
            }
        }
    }
}
