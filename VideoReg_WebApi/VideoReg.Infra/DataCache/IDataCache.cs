using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiServicePack
{
    public interface IDataCache<TKey, TValue>
    {
        /// <summary>
        /// Устанавливает или обновляет значение в коллекции по ключу
        /// </summary>
        /// <returns>Старый елемент коллекции при обновлении либо default если значения не было до установки</returns>
        CacheValue<TValue> AddOrUpdate(TKey key, TValue value);
        CacheValue<TValue> GetOrDefault(TKey key);
        CacheKeyValue<TKey, TValue>[] GetAll();
        CacheKeyValue<TKey, TValue>[] GetAllWhere(Func<CacheKeyValue<TKey, TValue>, bool> filter);
        CacheKeyValue<TKey, TValue>[] GetAllActual(int oldMs);
        CacheKeyValue<TKey, TValue>[] GetAllActualWhere(int oldMs, Func<CacheKeyValue<TKey, TValue>, bool> filter);
    }
}