using System;
using System.Collections.Generic;

namespace WebApi.Collection
{
    public interface ITimestampCollection<TKey, TValue>
    {
        bool TryGet(TKey key, out TimestampValue<TValue> value);
        bool TryGetActual(TKey key, TimeSpan actual, out TimestampValue<TValue> value);

        bool TryGetValue(TKey key, out TValue value);
        bool TryGetActualValue(TKey key, TimeSpan actual, out TValue value);

        IEnumerable<TimestampKeyValuePair<TKey, TValue>> GetAll();
        IEnumerable<TimestampKeyValuePair<TKey, TValue>> GetAllActual(TimeSpan actual);
        IEnumerable<TKey> GetKeys();
        IEnumerable<TKey> GetActualKeys(TimeSpan actual);
        /// <returns>true - add, update - false</returns>
        bool AddOrUpdate(TKey key, TValue value);
    }
}