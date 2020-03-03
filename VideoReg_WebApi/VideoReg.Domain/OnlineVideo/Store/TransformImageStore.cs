using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VideoReg.Domain.Archive.Config;
using VideoReg.Domain.Store;
using VideoReg.Infra.Services;

namespace VideoReg.Domain.OnlineVideo.Store
{
    class CameraImageTimestamp
    {
        public CameraImageTimestamp(int cameraNumber)
        {
            CameraNumber = cameraNumber;
        }
        public int CameraNumber { get; set; }
        public volatile byte[] NativeImg;
        public CameraImage ConvertedImg { get; set; }

        public byte[] GetConvertedIfPossibleOrNative() => ConvertedImg?.img ?? NativeImg;

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

        public Action<int, byte[]> OnImageChanged = (i, bytes) => {};

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
            var imgSettings = settings.GetOrDefault(cameraNumber);
            if (imgSettings.IsNotDefault())
                convertedImg = videoConvector.ConvertVideo(img, imgSettings);
            store.AddOrUpdate(cameraNumber, new CameraImage(imgSettings, convertedImg), img);
            
            Task.Run(() => OnImageChanged(cameraNumber, convertedImg));
            log.Info($"camera[{cameraNumber}] was updated. Converted={imgSettings.IsNotDefault()} ({imgSettings})");
        }

        /// <returns>return null if image is not exist</returns>
        /// <exception cref="NoNModifiedException">Then camera image exist but timestamp is same and all attentions is waited.</exception>
        public async Task<CameraResponse> GetCameraAsync(int cameraNumber, ImageTransformSettings imgSettings, DateTime? timeStamp)
        {
            if (imgSettings != null)
            {
                settings.Set(cameraNumber, imgSettings);
            }
            
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
                    byte[] imgBytes = img.ConvertedImg.img; // Устанавливаем конвертированное значение из кэша
                    if (imgSettings == null) // Настройки не переданны берем исходное изображение
                    {
                        imgBytes = img.NativeImg;
                    }
                    else if (!img.ConvertedImg.settings.Equals(imgSettings)) // переданные настройки не соответствуют настройкам в кэше.
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

        public IEnumerable<int> GetAvailableCameras() => store.GetAvailableCameras();
    }
}
