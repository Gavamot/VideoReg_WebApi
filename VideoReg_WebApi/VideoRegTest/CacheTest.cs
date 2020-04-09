using System;
using System.Collections.Concurrent;

namespace WebApiTest
{
    //public class CacheTest : IMemoryCache
    //{
    //    ConcurrentDictionary<object, object> cache = 
    //        new ConcurrentDictionary<object, object>();

    //    public void SetCache(ConcurrentDictionary<object, object> cache)
    //    {
    //        this.cache = cache;
    //    }

    //    void IDisposable.Dispose()
    //    {
    //        cache.Clear();
    //    }

    //    public bool TryGetValue(object key, out object value)
    //    {
    //       var res = cache.TryGetValue(key, out var v);
    //       value = v?.Value;
    //       return res;
    //    }

    //    public ICacheEntry CreateEntry(object key)
    //    {
    //        var entry = new CacheEntryTest();
    //        entry.Key = key;
    //        cache.AddOrUpdate(key, entry, (k, old) => entry);
    //        return entry;
    //    }

    //    public void Remove(object key)
    //    { 
    //        cache.TryRemove(key, out var obj);
    //    }
    //}
}
