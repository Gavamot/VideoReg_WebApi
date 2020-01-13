using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        private readonly Dictionary<TKey, CacheValue<TValue>> store
            = new Dictionary<TKey, CacheValue<TValue>>();

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

        private readonly object lockObj = new object();
        
        public CacheValue<TValue> AddOrUpdate(TKey key, TValue value)
        {
            var newValue = CreateValue(value);
            lock (lockObj)
            {
                if (store.ContainsKey(key))
                {
                    var old = store[key];
                    store[key] = newValue;
                    return old;
                }
                else
                {
                    store.Add(key, newValue);
                    return default;
                }
            }
        }

        /// <returns>Value if key exist or default(null) if not exist</returns>
        public CacheValue<TValue> GetOrDefault(TKey key)
        {
            CacheValue<TValue> item = default;
            lock (lockObj)
            {
                if (!store.TryGetValue(key, out item)) 
                    return default;
            }
            return item;
        }

        public CacheKeyValue<TKey, TValue>[] GetAll()
        {
            lock (lockObj)
            {
                return store.Select(CreateKeyValue).ToArray();
            }
        }

        public CacheKeyValue<TKey, TValue>[] GetAllWhere(Func<CacheKeyValue<TKey, TValue>, bool> filter)
        { 
            return GetAll().Where(filter).ToArray();
        }

        public CacheKeyValue<TKey, TValue>[] GetAllActual(int oldMs)
        {
            var now = dateService.GetNow();
            return GetAll().Where(x => (now - x.Timestamp).TotalMilliseconds <= oldMs).ToArray();
        }

        public CacheKeyValue<TKey, TValue>[] GetAllActualWhere(int oldMs, Func<CacheKeyValue<TKey, TValue>, bool> filter)
            => GetAllActual(oldMs).Where(filter).ToArray();
    }
}
