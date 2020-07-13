using System.Threading.Tasks;

namespace WebApi.OnlineVideo
{
    public interface ICameraHttpSourceRep
    {
        Task<CameraSourceHttpSettings[]> GetAll();
        Task<CameraSourceHttpSettings> Get(int cameraNumber);
    }
}
