using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ApiServicePack;
using Newtonsoft.Json.Converters;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.VideoRegInfo;
using VideoReg.Infra.Services;
using VideoRegService.Core;

namespace VideoRegService
{
    public class RedisVideoRegInfoRep : IVideoRegInfoRep
    {
        readonly IRedisRep redis;
        readonly IDateTimeService dateService;
        private readonly ICameraSourceRep cameraRep;

        public RedisVideoRegInfoRep(IRedisRep redis, IDateTimeService dateService, ICameraSourceRep cameraRep)
        {
            this.redis = redis;
            this.dateService = dateService;
            this.cameraRep = cameraRep;
        }

        public async Task<VideoRegInfoDto> GetInfo()
        {
            var res = await redis.Get<VideoRegInfoDto>("VideoRegInfo");
            var cameras = await cameraRep.GetAll();
            res.Cameras = cameras
                .Select(x => x.number)
                .ToArray();
            return res;
        }
    }
}
