using System.Linq;
using System.Threading.Tasks;
using WebApi.CoreService.Core;
using WebApi.OnlineVideo;

namespace WebApi.CoreService
{
    public class RedisCameraHttpSourceRep : ICameraHttpSourceRep
    {
        IRedisRep  redis;
        public RedisCameraHttpSourceRep(IRedisRep redis)
        {
            this.redis = redis;
        }

        public async Task<CameraSourceHttpSettings[]> GetAll() => await redis.GetObject<CameraSourceHttpSettings[]>("CamInfo");

        public async Task<CameraSourceHttpSettings> Get(int cameraNumber)
        {
            var settings = await GetAll();
            return settings.First(x => x.number == cameraNumber);
        }
    }
}
