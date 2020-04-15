using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using WebApi.Configuration;
using WebApi.Contract;
using WebApi.OnlineVideo.Store;
using WebApi.Core;
using WebApi.Services;

namespace WebApi.OnlineVideo.OnlineVideo
{
    public class VideoTransmitterHostedService : IHostedService
    {
        private readonly ILog log;
        private readonly IClientAscHub hub;
        private readonly ICameraStore cameraStore;
        private readonly ICameraSettingsStore settingsStore;
        private readonly IRegInfoRep regInfoRep;
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
            ILog log,
            IVideoTransmitterConfig config,
            IRegInfoRep regInfoRep,
            IClientAscHub hub,
            ICameraStore cameraStore,
            ICameraSettingsStore settingsStore)
        {
            this.config = config;
            this.cameraStore = cameraStore;
            this.settingsStore = settingsStore;
            this.regInfoRep = regInfoRep;
            this.hub = hub;
            this.log = log;
        }

        private void RegInfoChanged(RegInfo regInfo)
        {
            hub.SendNewRegInfoAsync(regInfo);
        }

        private int SendCameraImages()
        {
            var tasks = new List<Task>();
            for (int cameraNumber = FirstCamera; cameraNumber < CamerasArraySize; cameraNumber++)
            {
                var camNum = cameraNumber;
                if (!updatedCameras[camNum]) continue;
                var task= Task.Run(async () =>
                {
                    var img = cameraStore.GetOrDefaultTransformedImage(camNum);
                    if (img == default) return;
                    await hub.SendCameraImageAsync(camNum, img);
                    updatedCameras[camNum] = false;
                    log.Info($"ReceiveCameraImage[{camNum}]");
                });
                tasks.Add(task);
            }

            if(tasks.Any())
            {
                Task.WaitAll(tasks.ToArray());
                return tasks.Count;
            }
            return 0;
        }

        private async Task SendCameraImagesLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    int sendImages = SendCameraImages();
                    if (sendImages == 0)
                    {
                        await Task.Delay(5, cancellationToken);
                    }
                }
                catch (Exception e)
                {
                    log.Error($"can not send SendCameraImages message={e.Message}", e);
                    SwitchCamera(AllCameras, false);
                    await Task.Delay(500, cancellationToken);
                    await InitSession(cancellationToken);
                }
    
            }
        }

        private async Task TransmitDataLoopAsync(CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(config.AscRegServiceEndpoint))
            {
                log.Info($"VideoTransmitterService is off. config.AscRegServiceEndpoint is empty ");
                return;
            }
                
            log.Info($"VideoTransmitterService connect to {config.AscRegServiceEndpoint}");
            
            hub.OnInitShow = InitShow;
            await InitSession(cancellationToken);

            cameraStore.OnImageChanged = GotSnapshot;
            regInfoRep.RegInfoChanged = RegInfoChanged;
            
            await SendCameraImagesLoop(cancellationToken);
        }

        private async Task InitSession(CancellationToken cancellationToken)
        {
            log.Info($"Get reg info ...");
            RegInfo regInfo = await Must.Do(async () => await regInfoRep.GetInfoAsync(), 1000, cancellationToken, exception =>
            {
                log.Error($"VideoTransmitterService can not get RegInfo ({exception.Message})", exception);
            });

            await Must.Do(async () =>
            {
                await hub.ConnectWithRetryAsync();
                log.Info($"VideoTransmitterService InitSession() to {config.AscRegServiceEndpoint}");
                await hub.InitSessionAsync(regInfo);
            }, 1000, cancellationToken, exception =>
            {
                log.Error($"VideoTransmitterService ({exception.Message})", exception);
            });

            hub.OnStartShow = camera => SwitchCamera(camera, true);
            hub.OnStopShow = camera => SwitchCamera(camera, false);
            hub.OnSetCameraSettings = settings =>
            {
                log.Info($"Have got new camera[{settings.Camera}] settings {settings.Settings}");
                settingsStore.Set(settings);
            };
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                await TransmitDataLoopAsync(cancellationToken);
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
            regInfoRep.RegInfoChanged = info => {};
        }

        private void InitShow(CameraSettings[] cameras)
        {
            log.Info("server InitShow");
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
                log.Info($"All cameras - show {cameraStatus}");
                SwitchAllCameras(enabled);
            }
            else
            {
                log.Info($"{camera} camera - show {cameraStatus}");
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

        private void GotSnapshot(int camera, byte[] image)
        { 
            updatedCameras[camera] = enabledCameras[camera];
        }
    }
}
