using System;

namespace CacheDemo.Cache;

public interface ICache<TKey, TValue>
{
    int Capacity { get; }
    int Count { get; }

    bool TryGet(TKey key, out TValue value);
    void Set(TKey key, TValue value);
    TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory);
}

