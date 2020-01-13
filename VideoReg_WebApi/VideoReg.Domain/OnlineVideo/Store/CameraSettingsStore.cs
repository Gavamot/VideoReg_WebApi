using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace VideoReg.Domain.OnlineVideo.Store
{
    public interface ICameraSettingsStore
    {
        ImageTransformSettings Get(int cameraNumber);
        void Set(int cameraNumber, ImageTransformSettings setting);
    }

    public class CameraSettingsStore : ICameraSettingsStore
    {
        protected readonly ConcurrentDictionary<int, ImageTransformSettings> store
            = new ConcurrentDictionary<int, ImageTransformSettings>();

        public ImageTransformSettings Get(int cameraNumber)
        {
            return store.TryGetValue(cameraNumber, out var settings) ? settings : default;
        }

        public void Set(int cameraNumber, ImageTransformSettings setting)
        {
            store.AddOrUpdate(cameraNumber, k => setting, (k, oldV) => setting);
        }
    }
}
