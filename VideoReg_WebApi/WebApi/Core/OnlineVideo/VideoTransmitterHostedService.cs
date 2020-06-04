﻿using System;
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
        readonly AscHttpClient ascHttp;

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
        private volatile bool isInited = false;

        public VideoTransmitterHostedService(
            ILog log,
            IVideoTransmitterConfig config,
            IRegInfoRep regInfoRep,
            IClientAscHub hub,
            ICameraStore cameraStore,
            ICameraSettingsStore settingsStore,
            AscHttpClient ascHttp
            )
        {
            this.config = config;
            this.cameraStore = cameraStore;
            this.settingsStore = settingsStore;
            this.regInfoRep = regInfoRep;
            this.hub = hub;
            this.log = log;
            this.ascHttp = ascHttp;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            TransmitDataLoop();
            CheckConnectionLoop();
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

                var img = cameraStore.GetImage(camNum);
                if (img == default) continue;

                byte[] imgData = img.NativeImg;
                var settings = settingsStore.Get(camNum);
                if (settings != default)
                    imgData = settings.EnableConversion ? img.ConvertedImg.Image : img.NativeImg;

                int convertMs = img.ConvertedImg?.ConvertMs ?? 0;

                var task = Task.Run(async () =>
                {
                    var imageSended = await ascHttp.SendCameraImagesHttpAsync(regInfoRep.Vpn, camNum, imgData, convertMs);
                    if (imageSended)
                    {
                        updatedCameras[camNum] = false;
                        log.Info($"ReceiveCameraImage[{camNum}]");
                    }
                });
                tasks.Add(task);
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (Exception e)
            {
                log.Error($"!!! Bug VideoTransmitterHostedService.SendCameraImages fix it fast {e.Message}", e);
            }
            return tasks.Count;
        }

        private async Task SendCameraImagesLoop()
        {
            while (true)
            {
                int sendImages = SendCameraImages();
                if (sendImages == 0)
                {
                    await Task.Delay(5);
                }
            }
        }

        private void TransmitDataLoop()
        {
            Task.Factory.StartNew(async () =>
            {
                if (string.IsNullOrEmpty(config.AscRegServiceEndpoint))
                {
                    log.Info($"VideoTransmitterService is off. config.AscRegServiceEndpoint is empty ");
                    return;
                }

                log.Info($"VideoTransmitterService connect to {config.AscRegServiceEndpoint}");

                hub.OnInitShow = InitShow;
                await InitSessionAsync();

                cameraStore.OnImageChanged = GotSnapshot;
                regInfoRep.RegInfoChanged = RegInfoChanged;

                await SendCameraImagesLoop();
            }, TaskCreationOptions.LongRunning);
        }

        private async Task InitSessionAsync()
        {
            isInited = false;
            log.Info($"GetObject reg info ...");
            RegInfo regInfo = await Must.Do(async () => await regInfoRep.GetInfoAsync(), 1000, CancellationToken.None, exception =>
            {
                log.Error($"VideoTransmitterService can not get RegInfo ({exception.Message})", exception);
            });

            await Must.Do(async () =>
            {
                await hub.ConnectWithRetryAsync();
                log.Info($"VideoTransmitterService InitSessionAsync() to {config.AscRegServiceEndpoint}");
                await hub.InitSessionAsync(regInfo);
            }, 1000, CancellationToken.None, exception =>
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

        /// <summary>
        /// CheckConnectionLoop
        /// </summary>
        /// <exception cref="ObjectDisposedException">Ignore.</exception>
        /// <exception cref="InvalidOperationException">Ignore.</exception>
        private void CheckConnectionLoop()
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
                        catch (Exception e)
                        {
                            log.Error(e.Message, e);
                            await InitSessionAsync();
                        }
                    }
                }
            }, TaskCreationOptions.LongRunning);
            task.Start();
        }

        private void UnsubscribeHubForEvents()
        {
            hub.OnInitShow = cam => { };
            cameraStore.OnImageChanged = (i, bytes) => { };
            regInfoRep.RegInfoChanged = info => { };
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
                .Where(x => x.Enabled)
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
