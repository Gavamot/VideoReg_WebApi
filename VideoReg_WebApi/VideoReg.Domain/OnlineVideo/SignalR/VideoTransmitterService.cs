using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VideoReg.Domain.Archive.Config;
using VideoReg.Domain.OnlineVideo.Store;
using VideoReg.Domain.Store;
using VideoReg.Domain.Contract;

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
        private readonly IRegInfoRep regInfoReg;
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
            IRegInfoRep regInfoReg,
            IClientVideoHub hub,
            ICameraStore cameraStore,
            ICameraSettingsStore settingsStore)
        {
            this.config = config;
            this.cameraStore = cameraStore;
            this.settingsStore = settingsStore;
            this.regInfoReg = regInfoReg;
            this.hub = hub;
            this.log = log;
        }

        private void RegInfoChanged(RegInfo regInfo)
        { 
            hub.SendNewRegInfoAsync(regInfo);
        }

        private async Task TransmitDataLoopAsync()
        {
            await InitSession();

            while (true)
            {
                try
                {
                    for (int cameraNumber = FirstCamera; cameraNumber < CamerasCount; cameraNumber++)
                    {
                        if (!updatedCameras[cameraNumber]) continue;
                        var img = cameraStore.GetOrDefaultTransformedImage(cameraNumber);
                        if (img == default) continue;
                        await hub.SendCameraImageAsync(cameraNumber, img);
                        updatedCameras[cameraNumber] = false;
                        log.LogInformation($"ReceiveCameraImage[{cameraNumber}]");
                    }
                }
                catch (Exception e)
                {
                    await Task.Delay(500);
                    await InitSession();
                }
                await Task.Delay(10);
            }
        }

        private async Task InitSession()
        {
            while (true)
            {
                try
                {
                    OffCamera(AllCameras);
                    await hub.ConnectWithRetryAsync();
                    var reg = await regInfoReg.GetInfo();
                    await hub.InitSessionAsync(reg);
                    break;
                }
                catch(Exception e)
                {
                    log.LogError(e, "InitSession error with {ip}", config.AscRegServiceEndpoint);
                }
            }
            SubscribeHubForEvents();
        }

        private void SubscribeHubForEvents()
        {
            hub.OnInitShow += InitShow;
            cameraStore.OnImageChanged += GotSnapshot;
            regInfoReg.RegInfoChanged += RegInfoChanged;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await TransmitDataLoopAsync();
            }, cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            UnsubscribeHubForEvents();
            return Task.CompletedTask;
        }

        private void UnsubscribeHubForEvents()
        {
            hub.OnInitShow = cam => {};
            cameraStore.OnImageChanged = (i, bytes) => { };
            regInfoReg.RegInfoChanged = info => {};
        }

        private void InitShow(CameraSettings[] cameras)
        {
            log.LogInformation("server InitShow");
            InitCameraSettings(cameras);
            OnCameras(cameras);
        }

        private void InitCameraSettings(CameraSettings[] cameras)
        {
            foreach (var camera in cameras)
            {
                settingsStore.Set(camera);
            }
        }

        private void OnCameras(CameraSettings[] cameras)
        {
            var enabled = cameras.Select(x => x.Camera).ToArray();
            for (int i = FirstCamera; i < enabledCameras.Length; i++)
            {
                enabledCameras[i] = enabled.Contains(i);
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

       
    }
}
