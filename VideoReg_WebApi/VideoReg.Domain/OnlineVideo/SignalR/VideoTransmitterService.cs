using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoReg.Domain.Archive.Config;
using VideoReg.Domain.OnlineVideo.Store;
using VideoReg.Domain.Store;
using VideoReg.Domain.VideoRegInfo;

namespace VideoReg.Domain.OnlineVideo.SignalR
{
    // TODO : Сделать регуляровку частоты взятия снапшотов в зависимости от нагрузки процессора
    // TODO : Сделать переподключаемые хабы
    // TODO : Продумать концепцию сдлотов для отправки
    public class VideoTransmitterService : IHostedService
    {
        private readonly ILogger<VideoTransmitterService> log;
        private readonly IClientVideoHub hub;
        private readonly ICameraStore cameraStore;
        private readonly ICameraSettingsStore settingsStore;
        private readonly IVideoRegInfoStore regInfoStore;
        private readonly IVideoTransmitterConfig config;

        const int AllCameras = 0;
        private const int FirstCamera = 1;
        private const int CamerasCount = 10;
        /// <summary>
        /// Камеры по которым необходимо передавать изображения
        /// </summary>
        readonly bool[] enabledCameras = new bool[CamerasCount];
        /// <summary>
        /// Когда изображдение в cameraStore обновляется обновляется выставляется флаг, после отправки флаг снимается
        /// </summary>
        readonly bool[] updatedCameras = new bool[CamerasCount];

        public VideoTransmitterService(
            ILogger<VideoTransmitterService> log,
            IVideoTransmitterConfig config,
            IVideoRegInfoStore regInfoStore,
            IClientVideoHub hub,
            ICameraStore cameraStore,
            ICameraSettingsStore settingsStore)
        {
            this.config = config;
            this.cameraStore = cameraStore;
            this.settingsStore = settingsStore;
            this.regInfoStore = regInfoStore;
            this.hub = hub;
            this.log = log;
            hub.OnInitShow += InitShow;
            cameraStore.OnImageChanged += GotSnapshot;
            OffCamera(AllCameras);
        }

        private async Task TransmitDataLoop()
        {
            while (true)
            {
                for (int cameraNumber = FirstCamera; cameraNumber < CamerasCount; cameraNumber++)
                {
                    if (!updatedCameras[cameraNumber]) continue;
                    var img = cameraStore.GetOrDefaultTransformedImage(cameraNumber);
                    if(img == default) continue;
                    await hub.SendCameraImageAsync(cameraNumber, img);
                    updatedCameras[cameraNumber] = false;
                    log.LogInformation($"ReceiveCameraImage[{cameraNumber}]");
                }
                await Task.Delay(10);
            }
           
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await hub.ConnectAsync(config.AscRegServiceEndpoint, cancellationToken);
            var reg = regInfoStore.GetRegInfo();
            await hub.InitSessionAsync(reg);
            await TransmitDataLoop();
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void InitShow(int[] cameras)
        {
            log.LogInformation("server InitShow");
            for (int i = FirstCamera; i < enabledCameras.Length; i++)
            {
                enabledCameras[i] = cameras.Contains(i);
            }
        }

        private void OnCamera(int camera) => SwitchCamera(camera, true);

        private void OffCamera(int camera) => SwitchCamera(camera, false);

        private void SwitchCamera(int camera, bool enabled)
        {
            ThrowArgumentExceptionIfBadCamera(camera);

            if (camera == AllCameras)
            {
                SwitchAllCameras(enabled);
            }
            else
            {
                enabledCameras[camera] = enabled;
            }
        }

        private bool IsCorrectCamera(int camera) => camera >= AllCameras && camera < CamerasCount;

        private void ThrowArgumentExceptionIfBadCamera(int camera)
        {
            if (!IsCorrectCamera(camera))
            {
                throw new ArgumentException($"camera number must be in range ({0},{CamerasCount - 1})");
            }
        }

        private void SwitchAllCameras(bool enabled)
        {
            for (int i = FirstCamera; i < CamerasCount; i++)
            {
                enabledCameras[i] = enabled;
            }
        }

        private int[] GetEnabledCameras()
        {
            var res = new List<int>();
            for (int i = FirstCamera; i < CamerasCount; i++)
            {
                if (enabledCameras[i])
                {
                    res.Add(i);
                }
            }
            return res.ToArray();
        }

        private void GotSnapshot(int camera, byte[] image)
        { 
            updatedCameras[camera] = enabledCameras[camera];
        }

        private void SubscribeHubForEvents(IClientVideoHub hub)
        {
            

            // TODO : Подписатся на все события и дореализовать.
        }
    }
}
