using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ServiceStack.Redis;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.VideoRegInfo;

namespace VideoRegService.Core
{
    public interface IRedisRep
    {
        T Get<T>(string key);
        void Set<T>(string key, T value);
    }

    public class RedisRep : IRedisRep
    {
        public CameraSourceSettings[] CamInfo => Get<CameraSourceSettings[]>(nameof(CamInfo));
        public VideoRegInfoDto VideoRegInfo => Get<VideoRegInfoDto>(nameof(VideoRegInfo));

        RedisManagerPool pool;
        public RedisRep(RedisManagerPool pool)
        {
            this.pool = pool;
        }

        public const string DateTimeFormat = "d.M.yyyyTH:m:s";
        protected IsoDateTimeConverter GetIsoDateTimeConverter => new IsoDateTimeConverter { DateTimeFormat = DateTimeFormat };
        protected T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, GetIsoDateTimeConverter);
        protected string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, GetIsoDateTimeConverter);

        public T Get<T>(string key)
        {
            using var client = pool.GetClient();
            var res = client.Get<string>(key);
            return Deserialize<T>(res);
        }

        public void Set<T>(string key, T value)
        {
            using var client = pool.GetClient();
            var res = Serialize(value);
            client.Set(key, res);
        }
    }
}
