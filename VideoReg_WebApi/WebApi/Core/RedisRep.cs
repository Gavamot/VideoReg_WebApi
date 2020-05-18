﻿using System.Threading.Tasks;
using BeetleX.Redis;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

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
        private readonly string connection;

        public RedisRep(string connection)
        {
            this.connection = connection;
            Redis.Default.Host.AddWriteHost(connection);
        }

        public const string DateTimeFormat = "d.M.yyyyTH:m:s";
        protected IsoDateTimeConverter GetIsoDateTimeConverter => new IsoDateTimeConverter { DateTimeFormat = DateTimeFormat };
        protected T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, GetIsoDateTimeConverter);
        protected string Serialize<T>(T obj) => JsonConvert.SerializeObject(obj, GetIsoDateTimeConverter);

        public async Task<T> GetObject<T>(string key)
        {
            var json = await Redis.Get<string>(key);
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
        //    using var client = pool.GetClient();
        //    var res = Serialize(value);
        //    client.Set(key, res);
        //}
    }
}
