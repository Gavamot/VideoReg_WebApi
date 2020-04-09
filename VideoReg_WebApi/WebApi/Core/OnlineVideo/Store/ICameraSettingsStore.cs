using WebApi.Contract;

namespace WebApi.OnlineVideo.Store
{
    public interface ICameraSettingsStore
    {
        void SetAll(CameraSettings[] settings);
        ImageSettings GetOrDefault(int camera);
        void Set(CameraSettings settings);
        void Set(int camera, ImageSettings settings);
    }
}