using System.Collections.Concurrent;

namespace VideoReg.Domain.OnlineVideo.Store
{
    public interface ICameraSettingsStore
    {
        ImageTransformSettings GetOrDefault(int cameraNumber);
        void Set(int cameraNumber, ImageTransformSettings setting);
    }

    public class CameraSettingsStore : ICameraSettingsStore
    {
        protected readonly ConcurrentDictionary<int, ImageTransformSettings> store
            = new ConcurrentDictionary<int, ImageTransformSettings>();

        public CameraSettingsStore()
        {
            for (int i = 1; i < 10; i++)
            {
                store.TryAdd(i, new ImageTransformSettings
                {
                    Width = 640,
                    Height = 480
                });
            }   
        }

        public ImageTransformSettings GetOrDefault(int cameraNumber)
        {
            return store.TryGetValue(cameraNumber, out var settings) ? settings : new ImageTransformSettings();
        }

        public void Set(int cameraNumber, ImageTransformSettings setting)
        {
            store.AddOrUpdate(cameraNumber, k => setting, (k, oldV) => setting);
        }
    }
}
