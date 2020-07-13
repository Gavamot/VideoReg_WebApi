using System;
using System.Threading;
using System.Threading.Tasks;
using WebApi.Configuration;
using WebApi.OnlineVideo.Store;
using WebApi.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebApi.OnlineVideo
{
    public class CameraHttpHostedService : ServiceUpdater, IHostedService
    {
        private readonly IImgHttpRep imgRep;
        readonly ICameraStore cameraCache;
        readonly ICameraHttpSourceRep sourceRep;
        private ICameraConfig config;

        readonly CamerasInfoArray<bool> executingTask = new CamerasInfoArray<bool>(false);
        public override object Context { get; protected set; }
        public override string Name => "CameraUpdate";

        public CameraHttpHostedService(IImgHttpRep imgRep,
            ICameraStore cameraCache,
            ICameraHttpSourceRep cameraSourceRep,
            ICameraConfig config,
            ILogger<CameraHttpHostedService> log) : base(config.CameraUpdateIntervalMs, log)
        {
            this.imgRep = imgRep;
            this.cameraCache = cameraCache;
            this.sourceRep = cameraSourceRep;
            this.config = config;
        }

        /// <summary>
        /// DoWorkAsync
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        public override async Task DoWorkAsync(object context, CancellationToken cancellationToken)
        {
            await Measure.Invoke(async () =>
            {
                var settings = await sourceRep.GetAll();
                _ = Parallel.ForEach(settings, UpdateCameraImage);
            }, out var time);
            await SleepIfNeedMsAsync((long)time.TotalMilliseconds, cancellationToken);
        }

        public override Task<bool> BeforeStart(object context, CancellationToken cancellationToken) => Task.FromResult(true);

        async Task UpdateImage(Uri url, CameraSourceHttpSettings setting)
        {
            try
            {
                var img = await Measure.Invoke(async ()=> await imgRep.GetImgAsync(url, config.CameraGetImageTimeoutMs, CancellationToken.None), 
                    time => log.LogInformation($"imgRep.GetImgAsync time - {time.TotalMilliseconds}ms"));
                cameraCache.SetCamera(setting.number, img);
            }
            catch (HttpImgRepStatusCodeException e)
            {
                log.LogError($"Can not update camera[{setting.number}]({setting.snapshotUrl}) - {e.Message}", e);
                await Task.Delay(config.CameraUpdateSleepIfAuthorizeErrorTimeoutMs);
            }
            catch (Exception e)
            {
                log.LogError($"Can not update camera[{setting.number}]({setting.snapshotUrl}) - {e.Message}", e);
                await Task.Delay(config.CameraUpdateSleepIfErrorTimeoutMs);
            }
            finally
            {
                executingTask[setting.number] = false;
            }
        }

        protected void UpdateCameraImage(CameraSourceHttpSettings setting)
        {
            if (executingTask[setting.number]) return;
            if (!Uri.TryCreate(setting.snapshotUrl, UriKind.Absolute, out var uri))
            {
                log.LogError($"Camera[{setting.number}] has incorrect url {setting.snapshotUrl}");
                return;
            }
            executingTask[setting.number] = true;
            _ = UpdateImage(uri, setting);
        }
    }
}
