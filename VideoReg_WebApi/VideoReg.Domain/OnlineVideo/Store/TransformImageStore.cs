using System;
using System.Linq;
using System.Threading.Tasks;
using ApiServicePack;
using VideoReg.Domain.Archive.Config;
using VideoReg.Domain.Store;
using VideoReg.Infra.Services;

namespace VideoReg.Domain.OnlineVideo.Store
{
    public class TransformImageStore : ICameraStore
    {
        readonly ConcurrentDateCache<int, CameraImage> store;
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
            this.store = new ConcurrentDateCache<int, CameraImage>(dateService);
            this.config = config;
            this.log = log;
        }

        public void SetCamera(int cameraNumber, byte[] img)
        {
            byte[] convertedImg = img;
            var imgSettings = settings.GetOrDefault(cameraNumber);
            if (imgSettings.IsNotDefault())
                convertedImg = videoConvector.ConvertVideo(img, imgSettings);
            store.AddOrUpdate(cameraNumber, new CameraImage(imgSettings, convertedImg, img));
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
                if (img == default) // нет изображения в кэше
                    return default;
                
                if(img.Timestamp > timeStamp) // временная метка изображения. Возвращаем изображение
                {
                    byte[] imgBytes = img.Value.img; // Устанавливаем конвертированное значение из кэша
                    if (imgSettings.IsDefault()) // Настройки не переданны берем исходное изображение
                    {
                        imgBytes = img.Value.sourceImg;
                    }
                    else if (!img.Value.settings.Equals(imgSettings)) // переданные настройки не соответствуют настройкам в кэше.
                    {
                        imgBytes = videoConvector.ConvertVideo(img.Value.sourceImg, imgSettings);
                    }
                    return new CameraResponse
                    {
                        Img = imgBytes,
                        Timestamp = img.Timestamp
                    };
                }

                if (attempts == 0) // Последняя попытка
                    throw new NoNModifiedException();

                // в кеше изображение не обновлялось
                await Task.Delay(config.ImagePollingDelayMs);
            }

            return default;
        }

        public int[] GetAvailableCameras()
        {
            return store.GetAll()
                .Select(x => x.Key)
                .ToArray();
        }
    }
}
