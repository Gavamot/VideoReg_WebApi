using System.Collections.Concurrent;
using System.Reflection;

namespace VideoReg.Domain.OnlineVideo.Store
{
    public interface ICameraSettingsStore
    {
        ImageSettings GetOrDefault(int cameraNumber);
        void Set(int cameraNumber, ImageSettings setting);
    }

    public class CameraSettingsStore : ICameraSettingsStore
    {
        //protected readonly ConcurrentDictionary<int, ImageSettings> store
        //    = new ConcurrentDictionary<int, ImageSettings>();

        private readonly ImageSettings[] store = new ImageSettings[10]; 


        public CameraSettingsStore(CameraImage[] settings)
        {

            for (int i = 1; i < 10; i++)
            {
                store.TryAdd(i, new ImageSettings
                {
                    Width = 640,
                    Height = 480
                });
            }   
        }

        public ImageSettings GetOrDefault(int cameraNumber)
        {
            return store.TryGetValue(cameraNumber, out var settings) ? settings : new ImageSettings();
        }

        public void Set(int cameraNumber, ImageSettings setting)
        {
            store.AddOrUpdate(cameraNumber, k => setting, (k, oldV) => setting);
        }
    }
}
