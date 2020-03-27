using System;
using System.Collections.Generic;
using System.Linq;
using MustDo;
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
    public class VideoTransmitterHostedService : IHostedService
    {
        private readonly ILogger<VideoTransmitterHostedService> log;
        private readonly IClientVideoHub hub;
        private readonly ICameraStore cameraStore;
        private readonly ICameraSettingsStore settingsStore;
        private readonly IRegInfoRep regInfoReg;
        private readonly IVideoTransmitterConfig config;

        const int AllCameras = 0;
        private const int FirstCamera = 1;
        private const int CamerasArraySize = 10;
        
        /// <summary>
        /// Камеры по которым необходимо передавать изображения
        /// </summary>
        readonly bool[] enabledCameras = new bool[CamerasArraySize]; 
        /// <summary>
        /// Когда изображдение в cameraStore обновляется обновляется выставляется флаг, после отправки флаг снимается
        /// </summary>
        readonly bool[] updatedCameras = new bool[CamerasArraySize];

        public VideoTransmitterHostedService(
            ILogger<VideoTransmitterHostedService> log,
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

        private void SendCameraImages()
        {
            var tasks = new Task[CamerasArraySize - FirstCamera];
            for (int cameraNumber = FirstCamera; cameraNumber < CamerasArraySize; cameraNumber++)
            {
                var camNum = cameraNumber;
                tasks[camNum - FirstCamera] = Task.Run(async () =>
                {
                    if (!updatedCameras[camNum]) return;
                    var img = cameraStore.GetOrDefaultTransformedImage(camNum);
                    if (img == default) return;
                    await hub.SendCameraImageAsync(camNum, img);
                    updatedCameras[camNum] = false;
                    log.LogInformation($"ReceiveCameraImage[{camNum}]");
                });
            }
            Task.WaitAll(tasks);
            
        }

        private async Task SendCameraImagesLoop()
        {
            while (true)
            {
                try
                {
                    SendCameraImages();
                }
                catch (Exception e)
                {
                    log.LogError("can not send SendCameraImages message={message}", e, e.Message);
                    SwitchCamera(AllCameras, false);
                    await Task.Delay(500);
                    await InitSession();
                }
            }
        }

        private async Task TransmitDataLoopAsync()
        {
            if (string.IsNullOrEmpty(config.AscRegServiceEndpoint))
            {
                log.LogInformation($"VideoTransmitterService is off. config.AscRegServiceEndpoint is empty ");
                return;
            }
                
            log.LogInformation($"VideoTransmitterService connect to {config.AscRegServiceEndpoint}");
            
            hub.OnInitShow = InitShow;
            await InitSession();

            cameraStore.OnImageChanged = GotSnapshot;
            regInfoReg.RegInfoChanged = RegInfoChanged;
            
            await SendCameraImagesLoop();
        }

        private async Task InitSession()
        {
            var must = new Must(1000);
            must.OnError = exception => log.LogError(exception, exception.Message);
            
            var regInfo = await must.Exec(async() =>
            {
                log.LogInformation($"Get reg info ...");
                return await regInfoReg.GetInfoAsync();
            });

            await must.Exec(async ()=>
            {
                await hub.ConnectWithRetryAsync();
                log.LogInformation("VideoTransmitterService InitSession() to {ip}", config.AscRegServiceEndpoint);
                await hub.InitSessionAsync(regInfo);
            });

            hub.OnStartShow = camera => SwitchCamera(camera, true);
            hub.OnStopShow = camera => SwitchCamera(camera, false);
            hub.OnSetCameraSettings = settings =>
            {
                log.LogInformation($"Have got new camera[{settings.Camera}] settings {settings.Settings}");
                settingsStore.Set(settings);
            };
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

        private void SwitchCamera(int camera, bool enabled)
        {
            string cameraStatus = enabled ? "enabled" : "disabled";
            ThrowArgumentExceptionIfBadCamera(camera);
            
            if (camera == AllCameras)
            {
                log.LogInformation($"All cameras - show {cameraStatus}");
                SwitchAllCameras(enabled);
            }
            else
            {
                log.LogInformation($"{camera} camera - show {cameraStatus}");
                enabledCameras[camera] = enabled;
            }
        }

        private bool IsCorrectCamera(int camera) => camera >= AllCameras && camera < CamerasArraySize;

        private void ThrowArgumentExceptionIfBadCamera(int camera)
        {
            if (!IsCorrectCamera(camera))
            {
                throw new ArgumentException($"camera number must be in range ({0},{CamerasArraySize - 1})");
            }
        }

        private void SwitchAllCameras(bool enabled)
        {
            for (int i = FirstCamera; i < CamerasArraySize; i++)
            {
                enabledCameras[i] = enabled;
            }
        }

        private int[] GetEnabledCameras()
        {
            var res = new List<int>();
            for (int i = FirstCamera; i < CamerasArraySize; i++)
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
