using System.Threading.Tasks;

namespace WebApi.OnlineVideo
{
    public interface ICameraSourceRep
    {
        Task<CameraSourceSettings[]> GetAll();
        Task<CameraSourceSettings> Get(int cameraNumber);
    }
}
