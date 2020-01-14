using System.Linq;
using Newtonsoft.Json;
using ServiceStack.Redis;
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

        public VideoRegInfoDto GetInfo()
        {
            var res = redis.Get<VideoRegInfoDto>("VideoRegInfo");
            res.Cameras = cameraRep.GetAll()
                .Select(x => x.number)
                .ToArray();
            return res;
        }
    }
}
