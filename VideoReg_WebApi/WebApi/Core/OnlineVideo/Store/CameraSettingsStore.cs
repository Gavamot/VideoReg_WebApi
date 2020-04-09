using WebApi.Contract;

namespace WebApi.OnlineVideo.Store
{
    public class CameraSettingsStore : ICameraSettingsStore
    {
        public static ImageSettings DefaultSettings => new ImageSettings();
        private readonly CamerasInfoArray<ImageSettings> store = new CamerasInfoArray<ImageSettings>(DefaultSettings);

        public ImageSettings GetOrDefault(int camera)
        {
            return GetSettings(camera);
        }

        public void Set(CameraSettings setting)
        {
            store[setting.Camera] = setting.Settings;
        }

        public void Set(int camera, ImageSettings settings) => Set(new CameraSettings(camera, settings));

        public void SetAll(CameraSettings[] settings)
        {
            foreach (var s in settings)
            {
                store[s.Camera] = s.Settings;
            }
        }

        private ImageSettings GetSettings(int camera) => store[camera];

    }
}
