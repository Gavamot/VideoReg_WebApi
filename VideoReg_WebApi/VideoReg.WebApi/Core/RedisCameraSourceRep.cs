using System.Linq;
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

        public CameraSourceSettings[] GetAll() => redis.Get<CameraSourceSettings[]>("CamInfo");
        public CameraSourceSettings Get(int cameraNumber)
        {
            var settings = GetAll();
            return settings.First(x => x.number == cameraNumber);
        }
    }
}
