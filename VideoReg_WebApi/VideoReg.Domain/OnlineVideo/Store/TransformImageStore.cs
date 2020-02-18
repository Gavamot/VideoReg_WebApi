using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ApiServicePack;
using VideoReg.Domain.Archive.Config;
using VideoReg.Domain.Store;
using VideoReg.Infra.Services;

namespace VideoReg.Domain.OnlineVideo.Store
{
    class TimeIntervalCameraStatus
    {
        public DateTime? PrevTimestamp { get; set; }
        public DateTime? CurTimestamp { get; set; }

        private const int LifeTimeMs = 3000;

        public bool IsWork
        {
            get
            {
                if (PrevTimestamp == null || CurTimestamp == null)
                    return false;
                return (CurTimestamp.Value - PrevTimestamp.Value).TotalMilliseconds < LifeTimeMs;
            }
        }

        public void SetTimestamp(DateTime timestamp)
        {
            if (CurTimestamp == null)
            {
                CurTimestamp = timestamp;
            }
            else
            {
                PrevTimestamp = CurTimestamp;
                CurTimestamp = timestamp;
            }
        }

        public override string ToString() => $"{PrevTimestamp:yyyy.MM.dd HH:mm:ss.fff} - {CurTimestamp:yyyy.MM.dd HH:mm:ss.fff} = IsWork => { IsWork }";
    }

    class CameraImageTimestamp
    {
        public CameraImageTimestamp(int cameraNumber)
        {
            CameraNumber = cameraNumber;
        }
        public int CameraNumber { get; set; }
        public byte[] NativeImg { get; set; }
        public CameraImage ConvertedImg { get; set; }
        public readonly TimeIntervalCameraStatus cameraStatus = new TimeIntervalCameraStatus();

        public byte[] GetConvertedIfPossibleOrNative() => ConvertedImg?.img ?? NativeImg;
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
                camera.cameraStatus.SetTimestamp(now);
            }

            public List<int> GetAvailableCameras()
            {
                var res = new List<int>();
                for (int i = 0; i < store.Length; i++)
                {
                    if(store[i].cameraStatus.IsWork)
                        res.Add(store[i].CameraNumber);
                }
                return res;
            }

            public CameraImageTimestamp GetOrDefault(int cameraNumber)
            {
                var camera = store[cameraNumber];
                if (camera.cameraStatus.IsWork)
                    return camera;
                return default;
            }
        }

        private readonly TimeImageStore store;
        readonly IVideoConvector videoConvector;
        readonly IImagePollingConfig config;
        readonly ICameraSettingsStore settings;
        private readonly ILog log;

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

            store.AddOrUpdate(cameraNumber, new CameraImage(imgSettings, convertedImg, img), img);
            log.Info($"camera[{cameraNumber}] was updated. Converted={imgSettings.IsNotDefault()} ({imgSettings})");
        }

        /// <returns>return null if image is not exist</returns>
        /// <exception cref="NoNModifiedException">Then camera image exist but timestamp is same and all attentions is waited.</exception>
        public async Task<CameraResponse> GetCameraAsync(int cameraNumber, ImageTransformSettings imgSettings, DateTime timeStamp = default)
        {
            imgSettings ??= new ImageTransformSettings();
            settings.Set(cameraNumber, imgSettings);

            int attempts = config.ImagePollingAttempts;
            while (attempts-- > 0)
            {
                var img = store.GetOrDefault(cameraNumber);
                var _ = img?.cameraStatus?.CurTimestamp;
                if (_ == null) return default;
                DateTime curTimestamp = _.Value;

                if (curTimestamp != timeStamp) // временная метка изображения не соответвует переданной. Возвращаем изображение
                {
                    byte[] imgBytes = img.ConvertedImg.img; // Устанавливаем конвертированное значение из кэша
                    if (imgSettings.IsDefault()) // Настройки не переданны берем исходное изображение
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

        /// <returns>return null if image is not exist</returns>
        /// <exception cref="NoNModifiedException">Then camera image exist but timestamp is same and all attentions is waited.</exception>
        public async Task<CameraResponse> GetCameraFromCacheOrNativeAsync(int cameraNumber, DateTime timeStamp = default)
        {
            int attempts = config.ImagePollingAttempts;
            while (attempts-- > 0)
            {
                var img = store.GetOrDefault(cameraNumber);
                var _ = img?.cameraStatus?.CurTimestamp;
                if (_ == null) return default;
                DateTime curTimestamp = _.Value;

                if (curTimestamp != timeStamp) // временная метка изображения не соответвует переданной. Возвращаем изображение
                {
                    return new CameraResponse
                    {
                        Img = img.GetConvertedIfPossibleOrNative(),
                        Timestamp = curTimestamp
                    };
                }

                if (attempts == 0) // Последняя попытка
                  break;

                // в кеше изображение не обновлялось
                await Task.Delay(config.ImagePollingDelayMs);
            }
            throw new NoNModifiedException();
        }

        public IEnumerable<int> GetAvailableCameras() => store.GetAvailableCameras();
    }
}
