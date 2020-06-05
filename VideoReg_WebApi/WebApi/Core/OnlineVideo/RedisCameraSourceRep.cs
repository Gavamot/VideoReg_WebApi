using System.Linq;
using System.Threading.Tasks;
using WebApi.CoreService.Core;
using WebApi.OnlineVideo;

namespace WebApi.CoreService
{
    public class RedisCameraSourceRep : ICameraSourceRep
    {
        IRedisRep  redis;
        public RedisCameraSourceRep(IRedisRep redis)
        {
            this.redis = redis;
        }

        public async Task<CameraSourceSettings[]> GetAll() => await redis.GetObject<CameraSourceSettings[]>("CamInfo");

        public async Task<CameraSourceSettings> Get(int cameraNumber)
        {
            var settings = await GetAll();
            return settings.First(x => x.number == cameraNumber);
        }
    }
}
