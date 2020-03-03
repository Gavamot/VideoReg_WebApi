using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using VideoReg.Domain.Archive.Config;
using VideoReg.Domain.Store;
using VideoReg.Infra.Services;
using VideoRegService;

namespace VideoReg.Domain.OnlineVideo
{
    public class CameraUpdateService : ServiceUpdater
    {
        private readonly IImgRep imgRep;
        readonly ICameraStore cameraCache;
        readonly ICameraSourceRep sourceRep;
        private ICameraConfig config;
      
        readonly ConcurrentDictionary<int, CancellationTokenSource> tasks = new ConcurrentDictionary<int, CancellationTokenSource>();

      //  private readonly IServiceProvider di;
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
        }

        public override string Name => "CameraUpdate";


        protected void UpdateCameraImage(CameraSourceSettings setting)
        {
            async Task UpdateImage(Uri url, CameraSourceSettings setting, CancellationToken token)
            {
                try
                {
                    var img = await imgRep.GetImgAsync(url, config.CameraGetImageTimeoutMs, token);
                    cameraCache.SetCamera(setting.number, img);
                }
                catch (HttpImgRepStatusCodeException e)
                {
                    await Task.Delay(config.CameraUpdateSleepIfAuthorizeErrorTimeoutMs);
                    log.Error($"Can not update camera[{setting.number}]({setting.snapshotUrl}) - {e.Message}", e);
                }
                catch (Exception e)
                {
                    await Task.Delay(config.CameraUpdateSleepIfErrorTimeoutMs);
                    log.Error($"Can not update camera[{setting.number}]({setting.snapshotUrl}) - {e.Message}", e);
                }
                finally
                {
                    tasks.TryRemove(setting.number, out var task);
                }
            }

            if (tasks.ContainsKey(setting.number)) return;
            if (!Uri.TryCreate(setting.snapshotUrl, UriKind.Absolute, out var uri)) return;
            var tokenSource = new CancellationTokenSource();
            tasks.AddOrUpdate(setting.number, key => tokenSource, (key, old) =>
            {
                old.Cancel();
                return tokenSource;
            });
            var task = UpdateImage(uri, setting, tokenSource.Token);
        }

        public override async void DoWork(CancellationToken cancellationToken)
        {
            try
            {
                var settings = new[]
                {
                    new CameraSourceSettings(1, "http://192.168.88.10/tmpfs/auto.jpg"),
                    new CameraSourceSettings(2, "http://192.168.88.242/webcapture.jpg?command=snap&amp;channel=1"),
                    new CameraSourceSettings(3, "http://192.168.88.82/ISAPI/Streaming/channels/101/picture?snapShotImageType=JPEG")
                };
                //settings[0] = new CameraSourceSettings(settings[0].number, "http://192.168.88.54/webcapture.jpg?command=snap&amp;channel=0");
                //settings[1] = new CameraSourceSettings(settings[1].number, "http://192.168.88.242/webcapture.jpg?command=snap&amp;channel=0");
                //var settings = await sourceRep.GetAll();
                Parallel.ForEach(settings, UpdateCameraImage);
            }
            catch (Exception e)
            {
                await Task.Delay(config.CameraUpdateSleepIfErrorTimeoutMs, cancellationToken);
                log.Error($"Cannot find Cameras Settings ({e.Message})");
            }
        }
    }
}
