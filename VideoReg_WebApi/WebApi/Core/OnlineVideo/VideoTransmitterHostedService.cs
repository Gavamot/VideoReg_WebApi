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
using System.Net.Http;
using System.Text.Unicode;
using System.Text;

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

        string vpn = null;
        string Vpn
        {
            get
            {
                if (string.IsNullOrEmpty(vpn))
                {
                    vpn = regInfoRep.GetInfoAsync().Result.Vpn;
                }
                return vpn;
            }
        }

        /// <summary>
        /// Камеры по которым необходимо передавать изображения
        /// </summary>
        readonly bool[] enabledCameras = new bool[CamerasArraySize]; 
        /// <summary>
        /// Когда изображдение в cameraStore обновляется обновляется выставляется флаг, после отправки флаг снимается
        /// </summary>
        readonly bool[] updatedCameras = new bool[CamerasArraySize];
        readonly HttpClient http;
        readonly IDateTimeService dateTimeService;
        private volatile bool isInited = false; 

        public VideoTransmitterHostedService(
            ILog log,
            IVideoTransmitterConfig config,
            IRegInfoRep regInfoRep,
            IClientAscHub hub,
            ICameraStore cameraStore,
            ICameraSettingsStore settingsStore,
            IHttpClientFactory httpClientFactory,
            IDateTimeService dateTimeService)
        {
            this.config = config;
            this.cameraStore = cameraStore;
            this.settingsStore = settingsStore;
            this.regInfoRep = regInfoRep;
            this.hub = hub;
            this.log = log;
            http = httpClientFactory.CreateClient(Global.AscWebClient);
            this.dateTimeService = dateTimeService;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TransmitDataLoop(cancellationToken);
            CheckConnectionLoop(cancellationToken);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            UnsubscribeHubForEvents();
            return Task.CompletedTask;
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
                    var img = cameraStore.GetImage(camNum);
                    if (img == default) return;
                    var settings = settingsStore.Get(camNum);
                    byte[] imgData = settings.EnableConversion ?  img.ConvertedImg.Image : img.NativeImg;
                    await SendCameraImagesHttpAsync(camNum, imgData, img.ConvertedImg?.ConvertMs ?? 0);
                    //await hub.SendCameraImageAsync(camNum, imgData, img.ConvertedImg?.ConvertMs ?? 0);
                    updatedCameras[camNum] = false;
                    log.Info($"ReceiveCameraImage[{camNum}]");
                });
                tasks.Add(task);
            } 
            Task.WaitAll(tasks.ToArray()); 
            return tasks.Count;
        }

        private async Task SendCameraImagesHttpAsync(int cameraNumber, byte[] img, int convertedMs)
        {
            var content = new MultipartFormDataContent();
            content.Add(new StringContent(Vpn), "vpn");
            content.Add(new StringContent(cameraNumber.ToString()), "camera");
            var imgEncode = Convert.ToBase64String(img);
            content.Add(new StringContent(imgEncode), "file");
            content.Add(new StringContent(convertedMs.ToString()), "convertMs");
            var responce = await http.PostAsync(config.SetImageUrl, content);
            if (!responce.IsSuccessStatusCode)
            {
               
            }
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
                    await InitSessionAsync(cancellationToken);
                }
            }
        }

        private void TransmitDataLoop(CancellationToken token)
        {
            Task task = new Task(async () =>
            {
                if (string.IsNullOrEmpty(config.AscRegServiceEndpoint))
                {
                    log.Info($"VideoTransmitterService is off. config.AscRegServiceEndpoint is empty ");
                    return;
                }

                log.Info($"VideoTransmitterService connect to {config.AscRegServiceEndpoint}");

                hub.OnInitShow = InitShow;
                await InitSessionAsync(token);

                cameraStore.OnImageChanged = GotSnapshot;
                regInfoRep.RegInfoChanged = RegInfoChanged;

                await SendCameraImagesLoop(token);
            }, token, TaskCreationOptions.LongRunning);
            task.Start();
        }

        private async Task InitSessionAsync(CancellationToken cancellationToken)
        {
            isInited = false;
            log.Info($"Get reg info ...");
            RegInfo regInfo = await Must.Do(async () => await regInfoRep.GetInfoAsync(), 1000, cancellationToken, exception =>
            {
                log.Error($"VideoTransmitterService can not get RegInfo ({exception.Message})", exception);
            });

            await Must.Do(async () =>
            {
                await hub.ConnectWithRetryAsync();
                log.Info($"VideoTransmitterService InitSessionAsync() to {config.AscRegServiceEndpoint}");
                await hub.InitSessionAsync(regInfo);
            }, 1000, cancellationToken, exception =>
            {
                log.Error($"VideoTransmitterService ({exception.Message})", exception);
            });

            hub.OnStartShow = camera => SwitchCamera(camera, true);
            hub.OnStopShow = camera => SwitchCamera(camera, false);
            hub.OnSetCameraSettings = settings =>
            {
                log.Info($"Have got new camera[{settings.Camera}] Settings {settings.Settings}");
                settingsStore.Set(settings);
            };
            isInited = true;
        }

        private void CheckConnectionLoop(CancellationToken token)
        {
            Task task = new Task(async () =>
            {
                while (true)
                {
                    await Task.Delay(1000);
                    if (isInited)
                    {
                        try
                        {
                            await hub.SendTestConnectionAsync();
                        }
                        catch
                        {
                            await InitSessionAsync(token);
                        }
                        
                    }
                    token.ThrowIfCancellationRequested();
                }
            }, token, TaskCreationOptions.LongRunning);
            task.Start();
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
            InitEnableCameras(cameras);
        }

        private void InitEnableCameras(CameraSettings[] cameras)
        {
            var enabled = cameras
                .Where(x=> x.Enabled)
                .Select(x => x.Camera)
                .ToArray();
            for (int cameraNumber = FirstCamera; cameraNumber < enabledCameras.Length; cameraNumber++)
            {
                enabledCameras[cameraNumber] = enabled.Contains(cameraNumber);
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
