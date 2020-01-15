using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private volatile CameraSourceSettings[] camerasSettings = new CameraSourceSettings[0];

        private readonly ConcurrentDictionary<int, Task> tasks = new ConcurrentDictionary<int, Task>();

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
        public override void DoWork(CancellationToken cancellationToken)
        {
            try
            {
                camerasSettings = sourceRep.GetAll();
                UpdateCameraSettings(camerasSettings);
            }
            catch (Exception e)
            {
                log.Error($"Cannot find Cameras Settings ({e.Message})");
            }
        }

        private async void UpdateCamera(CameraSourceSettings camera)
        {
            try
            {
                var img = await imgRep.GetImgAsync(camera.snapshotUrl, config.CameraGetImageTimeoutMs);
                cameraCache.SetCamera(camera.number, img);
            }
            catch (Exception e)
            {
                log.Info($"{ServiceName} Can not update camera[{camera.number}] - {camera.snapshotUrl} ({e.Message})");
            }
            finally
            {
                tasks.TryRemove(camera.number, out var task);
            }

        }
        private void UpdateCameraSettings(IEnumerable<CameraSourceSettings> settings)
        {
            var cameras = settings as CameraSourceSettings[] ?? settings.ToArray();
            foreach (var camera in cameras)
            {
                if (tasks.ContainsKey(camera.number))
                    continue; // Предыдущая задача еще не выполнилась
                var task = new Task(() => UpdateCamera(camera));
                tasks.TryAdd(camera.number, task);
                task.Start();
            }
        }
    }
}
