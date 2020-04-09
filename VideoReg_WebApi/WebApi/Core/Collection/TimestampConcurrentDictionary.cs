using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using WebApi.Services;

namespace WebApi.Collection
{
    public class TimestampConcurrentDictionary<TKey, TValue> : ITimestampCollection<TKey, TValue>
    {
        readonly ConcurrentDictionary<TKey, TimestampKeyValuePair<TKey, TValue>> collection = new ConcurrentDictionary<TKey, TimestampKeyValuePair<TKey, TValue>>();
        private readonly IDateTimeService dateTimeService;
        public TimestampConcurrentDictionary(IDateTimeService dateTimeService)
        {
            this.dateTimeService = dateTimeService;
        }

        public bool TryGet(TKey key, out TimestampValue<TValue> value) 
        {
            if(collection.TryGetValue(key, out var v))
            {
                value = v.TimestampValue;
                return true;
            }
            value = null;
            return false;
        }
        
        public bool TryGetActual(TKey key, TimeSpan actual, out TimestampValue<TValue> value)
        {
            if (TryGet(key, out var item))
            {
                var old = dateTimeService.GetNow() - item.Timestamp;
                if (old <= actual)
                {
                    value = new TimestampValue<TValue>(item.Timestamp, item.Value);
                    return true;
                }
            }
            value = null;
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (TryGet(key, out var res))
            {
                value = res.Value;
                return true;
            }
            value = default;
            return false;
        }

        public bool TryGetActualValue(TKey key, TimeSpan actual, out TValue value)
        {
            if (TryGetActual(key, actual, out var res))
            {
                value = res.Value;
                return true;
            }
            value = default;
            return false;
        }

        public IEnumerable<TimestampKeyValuePair<TKey, TValue>> GetAll()
        {
            var res = new List<TimestampKeyValuePair<TKey, TValue>>();
            foreach (var item in collection)
            {
                res.Add(item.Value);
            }
            return res;
        }

        public IEnumerable<TimestampKeyValuePair<TKey, TValue>> GetAllActual(TimeSpan actual)
        {
            var res = new List<TimestampKeyValuePair<TKey, TValue>>();
            var now = dateTimeService.GetNow();
            foreach (var item in collection)
            {
                var old = now - item.Value.Timestamp;
                if (old <= actual)
                {
                    res.Add(item.Value);
                }
            }
            return res;
        }

        /// <returns> true-added, false-updated </returns>
        public bool AddOrUpdate(TKey key, TValue value)
        {
            var now = dateTimeService.GetNow();
            var newValue = new TimestampKeyValuePair<TKey, TValue>(now, key, value);
            var res = true;
            collection.AddOrUpdate(key, newValue, (key1, old) =>
            {
                res = false;
                return newValue;
            });
            return res;
        }

        public IEnumerable<TKey> GetKeys()
        {
            var res = new HashSet<TKey>();
            foreach (var item in collection)
            {
                res.Add(item.Key);
            }
            return res;
        }

        public IEnumerable<TKey> GetActualKeys(TimeSpan actual)
        {
            var res = new HashSet<TKey>();
            var now = dateTimeService.GetNow();
            foreach (var item in collection)
            {
                var old = now - item.Value.Timestamp;
                if (old <= actual)
                {
                    res.Add(item.Key);
                }
            }
            return res;
        }
    }
}
