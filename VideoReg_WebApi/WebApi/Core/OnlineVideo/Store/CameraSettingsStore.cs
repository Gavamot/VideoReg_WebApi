using System;
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

        public void Set(CameraSettings setting) => Set(setting.Camera, setting.EnableConversion, setting.Settings);

        public void Set(int camera, bool enableConversion, ImageSettings settings)
        {
            var cameraSettings = store[camera];
            cameraSettings.EnableConversion = enableConversion;
            cameraSettings.Settings = settings;
        }

        public void SetAll(CameraSettings[] settings)
        {
            foreach (var s in settings)
            {
                store[s.Camera] = s;
            }
        }

        private CameraSettings GetSettings(int camera) => store[camera];

    }
}
