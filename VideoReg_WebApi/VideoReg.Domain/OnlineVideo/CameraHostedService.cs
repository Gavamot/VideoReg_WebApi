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
    // TODO : переписать на нормальную реализацию для каждой камеры сделать индивидуальный подход.
    public class CameraHostedService : ServiceUpdater
    {
        private readonly IImgRep imgRep;
        readonly ICameraStore cameraCache;
        readonly ICameraSourceRep sourceRep;
        private ICameraConfig config;

        readonly CamerasInfoArray<bool> executingTask = new CamerasInfoArray<bool>(false);

        //  private readonly IServiceProvider di;
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

        public override string Name => "CameraUpdate";

        async Task UpdateImage(Uri url, CameraSourceSettings setting)
        {
            try
            {
                var img = await imgRep.GetImgAsync(url, config.CameraGetImageTimeoutMs, CancellationToken.None);
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

        public override async void DoWork(CancellationToken cancellationToken)
        {
            while (true)
            {
                try
                {
                    var settings = await sourceRep.GetAll();
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
}
