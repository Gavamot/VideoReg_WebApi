using VideoReg.Domain.Contract;

namespace VideoReg.Domain.OnlineVideo.Store
{
    public interface ICameraSettingsStore
    {
        void SetAll(CameraSettings[] settings);
        ImageSettings GetOrDefault(int camera);
        void Set(CameraSettings settings);
        void Set(int camera, ImageSettings settings);
    }

    public class CameraSettingsStore : ICameraSettingsStore
    {
        public static ImageSettings DefaultSettings => new ImageSettings();
        private readonly CamerasInfoArray<ImageSettings> store = new CamerasInfoArray<ImageSettings>();

        public CameraSettingsStore()
        {
            InitDefaultSettings();
        }
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

        private void InitDefaultSettings()
        {
            for (int i = store.firstCamera; i <= store.lastCamera; i++)
            {
                store[i] = DefaultSettings;
            }
        }
    }
}
