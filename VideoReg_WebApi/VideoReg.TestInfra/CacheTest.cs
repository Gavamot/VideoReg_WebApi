using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.Extensions.Caching.Memory;

namespace VideoReg.Infra.Test
{
    public class CacheTest : IMemoryCache
    {
        ConcurrentDictionary<object, ICacheEntry> cache = new ConcurrentDictionary<object, ICacheEntry>();

        public void SetCache(ConcurrentDictionary<object, ICacheEntry> cache)
        {
            this.cache = cache;
        }

        void IDisposable.Dispose()
        {
            cache.Clear();
        }

        public bool TryGetValue(object key, out object value)
        {
           var res = cache.TryGetValue(key, out var v);
           value = v?.Value;
           return res;
        }

        public ICacheEntry CreateEntry(object key)
        {
            var entry = new CacheEntryTest();
            entry.Key = key;
            cache.AddOrUpdate(key, entry, (k, old) => entry);
            return entry;
        }

        public void Remove(object key)
        {
            while (cache.ContainsKey(key))
            {
                cache.TryRemove(key, out var obj);
            }
        }
    }
}
