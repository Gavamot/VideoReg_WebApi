using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
using VideoReg.Infra.Services;

namespace ApiServicePack
{
    public class ConcurrentDateCache<TKey, TValue> : IDataCache<TKey, TValue>
    {
        private readonly IDateTimeService dateService;
        public ConcurrentDateCache(IDateTimeService dateService)
        {
            this.dateService = dateService;
        }

        private readonly ConcurrentDictionary<TKey, CacheValue<TValue>> store
            = new ConcurrentDictionary<TKey, CacheValue<TValue>>();

        private CacheValue<TValue> CreateValue(TValue value)
        {
            var timestamp = dateService.GetNow();
            return new CacheValue<TValue>(value, timestamp);
        }
        
        private CacheKeyValue<TKey, TValue> CreateKeyValue(TKey key, CacheValue<TValue> value)
        {
            return new CacheKeyValue<TKey, TValue>
            {
                Key = key,
                Value = value.Value,
                Timestamp = value.Timestamp
            };
        }
        private CacheKeyValue<TKey, TValue> CreateKeyValue(KeyValuePair<TKey, CacheValue<TValue>> x)
        {
            return new CacheKeyValue<TKey, TValue>
            {
                Key = x.Key,
                Value = x.Value.Value,
                Timestamp = x.Value.Timestamp
            };
        }

        public CacheValue<TValue> AddOrUpdate(TKey key, TValue value)
        {
            var newValue = CreateValue(value);
            return store.AddOrUpdate(key, k => newValue, (k, old) => newValue);
        }

        /// <returns>Value if key exist or default(null) if not exist</returns>
        public CacheValue<TValue> GetOrDefault(TKey key)
        {
            return store.TryGetValue(key, out var item) ? item : default;
        }

        private IEnumerable<CacheKeyValue<TKey, TValue>> All => store.ToArray().Select(CreateKeyValue);

        public CacheKeyValue<TKey, TValue>[] GetAll() => All.ToArray();

        public CacheKeyValue<TKey, TValue>[] GetAllWhere(Func<CacheKeyValue<TKey, TValue>, bool> filter) => All.Where(filter).ToArray();

        public CacheKeyValue<TKey, TValue>[] GetAllActual(int oldMs)
        {
            var now = dateService.GetNow();
            return All.Where(x => (now - x.Timestamp).TotalMilliseconds <= oldMs).ToArray();
        }

        public CacheKeyValue<TKey, TValue>[] GetAllActualWhere(int oldMs, Func<CacheKeyValue<TKey, TValue>, bool> filter)
            => GetAllActual(oldMs).Where(filter).ToArray();
    }
}
