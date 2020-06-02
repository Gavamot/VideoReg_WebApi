using System;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Configuration;
using WebApi.OnlineVideo.Store;
using WebApi.Services;
using Microsoft.Extensions.Hosting;

namespace WebApi.OnlineVideo
{
    public class CameraHostedService : ServiceUpdater, IHostedService
    {
        private readonly IImgRep imgRep;
        readonly ICameraStore cameraCache;
        readonly ICameraSourceRep sourceRep;
        private ICameraConfig config;

        readonly CamerasInfoArray<bool> executingTask = new CamerasInfoArray<bool>(false);
        public override object Context { get; protected set; }
        public override string Name => "CameraUpdate";

        public CameraHostedService(IImgRep imgRep,
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

        public override async Task DoWorkAsync(object context, CancellationToken cancellationToken)
        {
            await Measure.Invoke(async () =>
            {
                var settings = await sourceRep.GetAll();
                Parallel.ForEach(settings, UpdateCameraImage);
            }, out var time);
            await SleepIfNeedMsAsync((long)time.TotalMilliseconds, cancellationToken);
        }

        public override Task<bool> BeforeStart(object context, CancellationToken cancellationToken) => Task.FromResult(true);

        async Task UpdateImage(Uri url, CameraSourceSettings setting)
        {
            try
            {
                var img = await Measure.Invoke(async ()=> await imgRep.GetImgAsync(url, config.CameraGetImageTimeoutMs, CancellationToken.None), 
                    time => log.Info($"imgRep.GetImgAsync time - {time.TotalMilliseconds}ms"));
                cameraCache.SetCamera(setting.number, img);
            }
            catch (HttpImgRepStatusCodeException e)
            {
                log.Error($"Can not update camera[{setting.number}]({setting.snapshotUrl}) - {e.Message}", e);
                await Task.Delay(config.CameraUpdateSleepIfAuthorizeErrorTimeoutMs);
            }
            catch (Exception e)
            {
                log.Error($"Can not update camera[{setting.number}]({setting.snapshotUrl}) - {e.Message}", e);
                await Task.Delay(config.CameraUpdateSleepIfErrorTimeoutMs);
            }
            finally
            {
                executingTask[setting.number] = false;
            }
        }

        protected void UpdateCameraImage(CameraSourceSettings setting)
        {
            if (executingTask[setting.number]) return;
            if (!Uri.TryCreate(setting.snapshotUrl, UriKind.Absolute, out var uri))
            {
                log.Error($"Camera[{setting.number}] has incorrect url {setting.snapshotUrl}");
                return;
            }
            executingTask[setting.number] = true;
            _ = UpdateImage(uri, setting);
        }
    }
}
