using WebApi.Contract;

namespace WebApi.OnlineVideo.Store
{
    public interface ICameraSettingsStore
    {
        CameraSettings Get(int camera);
        CameraSettings[] GetAll();
        void Set(CameraSettings settings);
        void Set(int camera, bool enableConversion, ImageSettings settings);
        void Set(int camera, bool enableConversion);
    }
}