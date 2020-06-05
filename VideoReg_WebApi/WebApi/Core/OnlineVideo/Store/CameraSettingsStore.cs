using System.Linq;
using WebApi.Contract;

namespace WebApi.OnlineVideo.Store
{
    public class CameraSettingsStore : ICameraSettingsStore
    {
        private static readonly CameraSettings[] defaultSettings = Enumerable.Range(1, 9).Select(x => new CameraSettings
        {
            Camera = x,
            Enabled = true,
            EnableConversion = false,
            Settings = new ImageSettings()
        }).ToArray();

        private readonly CamerasInfoArray<CameraSettings> store = new CamerasInfoArray<CameraSettings>(defaultSettings);

        public CameraSettings Get(int camera)
        {
            return GetSettings(camera);
        }

        public CameraSettings[] GetAll() => store.GetAll();

        public void Set(CameraSettings newSetting)
        {
            if (newSetting.IsSettingsForAllCameras)
            {
                foreach (var settings in store.GetAll())
                {
                    settings.Update(newSetting);
                }
            }
            else
            {
                var cameraSettings = store[newSetting.Camera];
                cameraSettings.Update(newSetting);
            }
        }

        public void Set(int camera, bool enableConversion, ImageSettings settings)
        {
            var cameraSettings = store[camera];
            cameraSettings.EnableConversion = enableConversion;
            cameraSettings.Settings.Update(settings);
        }

        public void Set(int camera, bool enableConversion)
        {
            var cameraSettings = store[camera];
            cameraSettings.EnableConversion = enableConversion;
        }

        private CameraSettings GetSettings(int camera) => store[camera];

    }
}
