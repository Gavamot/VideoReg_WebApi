using System;

namespace ApiServicePack
{
    public class CacheKeyValue<TKey, TValue>
    {
        public DateTime Timestamp { get; set; }
        public TValue Value { get; set; }
        public TKey Key { get; set; }
    }
}