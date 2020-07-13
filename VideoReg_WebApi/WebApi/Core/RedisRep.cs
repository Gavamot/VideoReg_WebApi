using System.Threading.Tasks;
using BeetleX.Redis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using WebApi.Configuration;

namespace WebApi.CoreService.Core
{
    public interface IRedisRep
    {
        Task<T> GetObject<T>(string key);
        Task<string> GetString(string key);
        //void Set<T>(string key, T value);
    }

    public class RedisRep : IRedisRep
    {
        public RedisRep(IConfig config)
        {
            Redis.Default.Host.AddWriteHost(config.Redis);
        }

        protected readonly MultiFormatDateConverter converter = new MultiFormatDateConverter();
        protected T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, converter);
        protected string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, converter);

        public async Task<T> GetObject<T>(string key) 
        {
            var json = await Redis.Get<string>(key);
            if (json == null) return default;
            var res = Deserialize<T>(json);
            return res;
        }

        public async Task<string> GetString(string key)
        {
            var res = await Redis.Get<string>(key);
            return res;
        }

        //public void Set<T>(string key, T value)
        //{
        //    using var http = pool.GetClient();
        //    var res = Serialize(value);
        //    http.Set(key, res);
        //}
    }
}
