using System;

namespace ApiServicePack
{
    public class CacheValue<TValue>
    {
        public CacheValue(TValue value, DateTime timestamp)
        {
            Value = value;
            Timestamp = timestamp;
        }

        public readonly TValue Value;
        public readonly DateTime Timestamp;

    }
}