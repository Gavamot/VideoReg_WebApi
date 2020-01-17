using System.Linq;
using System.Threading.Tasks;
using VideoReg.Domain.OnlineVideo;
using VideoRegService.Core;

namespace VideoRegService
{
    public class RedisCameraSourceRep : ICameraSourceRep
    {
        IRedisRep  redis;
        public RedisCameraSourceRep(IRedisRep redis)
        {
            this.redis = redis;
        }

        public async Task<CameraSourceSettings[]> GetAll() => await redis.Get<CameraSourceSettings[]>("CamInfo");
        public async Task<CameraSourceSettings> Get(int cameraNumber)
        {
            var settings = await GetAll();
            return settings.First(x => x.number == cameraNumber);
        }
    }
}
