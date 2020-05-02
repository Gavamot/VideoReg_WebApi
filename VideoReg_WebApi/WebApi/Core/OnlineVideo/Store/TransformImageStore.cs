using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using WebApi.Configuration;
using WebApi.Contract;
using WebApi.Core;
using WebApi.Services;

namespace WebApi.OnlineVideo.Store
{
    public class CameraImageTimestamp
    {
        public CameraImageTimestamp(int cameraNumber)
        {
            CameraNumber = cameraNumber;
        }
        public int CameraNumber { get; set; }
        public volatile byte[] NativeImg;
        public CameraImage ConvertedImg { get; set; }

        public byte[] GetConvertedIfPossibleOrNative() => ConvertedImg?.Image ?? NativeImg;

        public DateTime? Timestamp { get; private set; }

        private const int LifeTimeMs = 3000;

        public bool IsWork
        {
            get
            {
                if (Timestamp == null)
                    return false;
                return (DateTime.Now - Timestamp.Value).TotalMilliseconds < LifeTimeMs;
            }
        }

        public void SetTimestamp(DateTime timestamp)
        {
            Timestamp = timestamp;
        }
    }

    public class TransformImageStore : ICameraStore
    {
        class TimeImageStore
        {
            readonly IDateTimeService dateTimeService;
            readonly CameraImageTimestamp[] store;
            public TimeImageStore(IDateTimeService dateTimeService)
            {
                this.dateTimeService = dateTimeService;
                this.store = InitStore();
            }

            private CameraImageTimestamp[] InitStore()
            {
                const int size = 10;
                var res = new CameraImageTimestamp[size];
                for (int i = 0; i < size; i++)
                {
                    res[i] = new CameraImageTimestamp(i);
                }
                return res;
            }

            public void AddOrUpdate(int cameraNumber, CameraImage convertedImg, byte[] nativeImg)
            {
                var camera = store[cameraNumber];
                camera.ConvertedImg = convertedImg;
                camera.NativeImg = nativeImg;
                DateTime now = dateTimeService.GetNow();
                camera.SetTimestamp(now);
            }

            public List<int> GetAvailableCameras()
            {
                var res = new List<int>();
                for (int i = 0; i < store.Length; i++)
                {
                    if(store[i].IsWork)
                        res.Add(store[i].CameraNumber);
                }
                return res;
            }

            public CameraImageTimestamp GetOrDefault(int cameraNumber)
            {
                var camera = store[cameraNumber];
                if (camera.IsWork)
                    return camera;
                return default;
            }
        }

        private readonly TimeImageStore store;
        readonly IVideoConvector videoConvector;
        readonly IImagePollingConfig config;
        readonly ICameraSettingsStore settings;
        private readonly ILog log;
        public Action<int, byte[]> OnImageChanged { get; set; }

        public TransformImageStore(IDateTimeService dateService,
            ICameraSettingsStore cameraSettingsStore,
            IVideoConvector videoConvector,
            ILog log,
            IImagePollingConfig config)
        {
            this.settings= cameraSettingsStore;
            this.videoConvector = videoConvector;
            this.config = config;
            this.log = log;
            this.store = new TimeImageStore(dateService);
        }

        public void SetCamera(int cameraNumber, byte[] img)
        {
            byte[] convertedImg = img;
            var imgSettings = settings.Get(cameraNumber);
            
            int convertMs = 0;

            if (imgSettings.EnableConversion && !imgSettings.Settings.IsDefaultSettings)
            {
                convertedImg = Measure.Invoke(() => videoConvector.ConvertVideo(img, imgSettings.Settings), out var time);
                log.Info($"camera[{cameraNumber}] ConvertVideo duration - {time.TotalMilliseconds}ms");
                convertMs = (int)time.TotalMilliseconds;
            }
            var image = new CameraImage(imgSettings.Settings, convertedImg, convertMs);

            store.AddOrUpdate(cameraNumber, image, img);

            OnImageChanged?.Invoke(cameraNumber, convertedImg);

            log.Info($"camera[{cameraNumber}] was updated. Converted={imgSettings.Settings.IsNotDefault()} ({imgSettings})");
        }

        /// <returns>return null if image is not exist</returns>
        /// <exception cref="NoNModifiedException">Then camera image exist but timestamp is same and all attentions is waited.</exception>
        public async Task<CameraResponse> GetCameraAsync(int cameraNumber, ImageSettings imgSettings, DateTime? timeStamp)
        {
            int attempts = config.ImagePollingAttempts;
            while (attempts-- > 0)
            {
                var img = store.GetOrDefault(cameraNumber);
                var _ = img?.Timestamp;
                if (_ == null) return default;
                if (!img.IsWork) return default;
                DateTime curTimestamp = _.Value;

                if (timeStamp == null || curTimestamp != timeStamp) // временная метка изображения не соответвует переданной. Возвращаем изображение
                {
                    byte[] imgBytes = img.ConvertedImg.Image; // Устанавливаем конвертированное значение из кэша
                    if (imgSettings == null) // Настройки не переданны берем исходное изображение
                    {
                        imgBytes = img.NativeImg;
                    }
                    else if (!img.ConvertedImg.Settings.Equals(imgSettings)) // переданные настройки не соответствуют настройкам в кэше.
                    {
                        imgBytes = videoConvector.ConvertVideo(img.NativeImg, imgSettings);
                    }
                    return new CameraResponse
                    {
                        Img = imgBytes,
                        Timestamp = curTimestamp
                    };
                }

                if (attempts != 0) // Последняя попытка
                    break;

                // в кэше изображение не обновлялось
                await Task.Delay(config.ImagePollingDelayMs);
            }
            throw new NoNModifiedException();
        }

        public CameraImageTimestamp GetImage(int cameraNumber)
        {
            return store.GetOrDefault(cameraNumber);
        }

        public IEnumerable<int> GetAvailableCameras() => store.GetAvailableCameras();
    }
}
