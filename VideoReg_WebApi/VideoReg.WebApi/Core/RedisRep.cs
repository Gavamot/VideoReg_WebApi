using System;
using System.Net.Http;
using System.Threading.Tasks;
using BeetleX.Redis;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VideoReg.Domain.OnlineVideo;
using VideoReg.Domain.VideoRegInfo;
using VideoReg.Infra.Services;

namespace VideoRegService.Core
{
    public interface IRedisRep
    {
        Task<T> Get<T>(string key);
        //void Set<T>(string key, T value);
    }

    public class RedisRep : IRedisRep
    {
        private readonly string connection;
        private readonly RedisDB DB;
        public RedisRep(string connection)
        {
            this.connection = connection;
            Redis.Default.Host.AddWriteHost(connection);
        }

        public const string DateTimeFormat = "d.M.yyyyTH:m:s";
        protected IsoDateTimeConverter GetIsoDateTimeConverter => new IsoDateTimeConverter { DateTimeFormat = DateTimeFormat };
        protected T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, GetIsoDateTimeConverter);
        protected string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, GetIsoDateTimeConverter);

        public async Task<T> Get<T>(string key)
        {
            var json = await Redis.Get<string>(key);
            var res = Deserialize<T>(json);
            return res;
        }

        //public void Set<T>(string key, T value)
        //{
        //    using var client = pool.GetClient();
        //    var res = Serialize(value);
        //    client.Set(key, res);
        //}
    }
}
