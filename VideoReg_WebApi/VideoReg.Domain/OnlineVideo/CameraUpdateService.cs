using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
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
        public override async void DoWork(CancellationToken cancellationToken)
        {
            try
            {
                camerasSettings = await sourceRep.GetAll();
                UpdateCameraSettings(camerasSettings);
            }
            catch (Exception e)
            {
                await Task.Delay(config.CameraUpdateSleepIfErrorTimeoutMs, cancellationToken);
                log.Error($"Cannot find Cameras Settings ({e.Message})");
            }
        }

        private void UpdateCamera(CameraSourceSettings camera)
        {
            try
            {
                if (Uri.TryCreate(camera.snapshotUrl, UriKind.Absolute, out var url))
                {
                    var img = imgRep.GetImg(url, config.CameraGetImageTimeoutMs);
                    cameraCache.SetCamera(camera.number, img);
                }
                else
                {
                    log.Warning($"camera[{camera.number}] has bad uri format {camera.snapshotUrl}");
                }
            }
            catch (HttpImgRepStatusCodeException e)
            {
                Thread.Sleep(config.CameraUpdateSleepIfAuthorizeErrorTimeoutMs);
                //Task.Delay(config.CameraUpdateSleepIfAuthorizeErrorTimeoutMs);
                log.Warning(
                    $"{ServiceName} Got bad response code ({e.Message}) camera[{camera.number}] - {camera.snapshotUrl} ({e.Message})");
            }
            catch (Exception e)
            {
                Thread.Sleep(config.CameraUpdateSleepIfErrorTimeoutMs);
                //Task.Delay(config.CameraUpdateSleepIfErrorTimeoutMs);
                log.Warning(
                    $"{ServiceName} Can not update camera[{camera.number}] - {camera.snapshotUrl} ({e.Message})");
            }
            finally
            {
                tasks.TryRemove(camera.number, out var task);
            }
        }

        private void UpdateCameraSettings(CameraSourceSettings[] settings)
        {
            foreach (var camera in settings)
            {
                if (tasks.ContainsKey(camera.number))
                    continue; // Предыдущая задача еще не выполнилась
                var task = new Task(()=> UpdateCamera(camera));
                tasks.TryAdd(camera.number, task);
                task.Start();
            }
        }
    }
}
