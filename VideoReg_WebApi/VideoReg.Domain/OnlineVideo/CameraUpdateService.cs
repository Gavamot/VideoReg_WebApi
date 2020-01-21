using System;
using System.Diagnostics;
using System.Linq;
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
        readonly IImgRep imgRep;
        readonly ICameraStore cameraCache;
        readonly ICameraSourceRep sourceRep;
        private ICameraConfig config;
        private static volatile CameraSourceSettings[] camerasSettings = new CameraSourceSettings[0];
        public CameraUpdateService(IImgRep imgRep,
            ICameraStore cameraCache,
            ICameraSourceRep cameraSourceRep,
            ICameraConfig config,
            ILog log) : base(config.CameraUpdateIntervalMs, log)
        {
            this.imgRep = imgRep;
            this.cameraCache = cameraCache;
            this.sourceRep = cameraSourceRep;
            this.config = config;
            BeforeStart += StartCameraThreads;
        }

        public override string Name => "CameraUpdate";

        protected void StartCameraThreads()
        {
            //bool GetUriWithAntiCacheParameter(string snapshotUrl, out Uri inUrl)
            //{
            //    string AddAntiCacheParameter(string uri)
            //    {
            //        TimeSpan now = DateTime.Now - DateTime.MinValue;
            //        string url = uri + $"&_rnd={now.TotalMilliseconds}";
            //        return url;
            //    }
            //    string antiCacheUri = AddAntiCacheParameter(snapshotUrl);
            //    bool res = Uri.TryCreate(antiCacheUri, UriKind.Absolute, out var url);
            //    inUrl = url;
            //    return res;
            //}

            void UpdateCamera(int cameraNumber)
            {
                var settings = camerasSettings.FirstOrDefault(x => x.number == cameraNumber);
                if (settings.number == 0) return;
                //if (GetUriWithAntiCacheParameter(settings.snapshotUrl, out Uri url))
                if(Uri.TryCreate(settings.snapshotUrl, UriKind.RelativeOrAbsolute, out var url))
                {
                    try
                    {
                        byte[] img = imgRep.GetImg(url, config.CameraGetImageTimeoutMs);
                        cameraCache.SetCamera(cameraNumber, img);
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
                var starter = new ThreadStart(() =>
                {
                    while (true)
                    {
                        var watcher = new Stopwatch();
                        watcher.Start();
                        UpdateCamera(camNum);
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
