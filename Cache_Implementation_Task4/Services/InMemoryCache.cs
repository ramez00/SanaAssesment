using Cache_Implementation_Task4.Interfaces;
using System;
using System.Collections.Generic;

namespace Cache_Implementation_Task4.Services;
public class InMemoryCache<TKey, TValue> : ICache<TKey, TValue>
{
    private sealed record CacheEntry(TValue Value);

    private readonly Dictionary<TKey, CacheEntry> _store;
    private readonly IEvictionPolicy<TKey> _evictionPolicy;
    private readonly object _lock = new();

    public InMemoryCache(int capacity,
                        IEvictionPolicy<TKey>? evictionPolicy = null,
                        IEqualityComparer<TKey>? comparer = null)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

        Capacity = capacity;
        _store = new Dictionary<TKey, CacheEntry>(capacity, comparer);
        _evictionPolicy = evictionPolicy ?? new EvictionPolicy<TKey>(comparer);
    }

    public int Capacity { get; }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _store.Count;
            }
        }
    }

    public bool TryGet(TKey key, out TValue value)
    {
        lock (_lock)
        {
            if (_store.TryGetValue(key, out var entry))
            {
                _evictionPolicy.RecordAccess(key);
                value = entry.Value;
                return true;
            }
        }

        value = default!;
        return false;
    }

    public void Set(TKey key, TValue value)
    {
        lock (_lock)
        {
            if (_store.ContainsKey(key))
            {
                _store[key] = new CacheEntry(value);
                _evictionPolicy.RecordAccess(key);
                return;
            }

            if (_store.Count >= Capacity && _evictionPolicy.TryEvict(out var evictedKey))
            {
                _store.Remove(evictedKey);
            }

            _store[key] = new CacheEntry(value);
            _evictionPolicy.RecordAdd(key);
        }
    }

    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        if (valueFactory is null) throw new ArgumentNullException(nameof(valueFactory));

        lock (_lock)
        {
            if (_store.TryGetValue(key, out var existing))
            {
                _evictionPolicy.RecordAccess(key);
                return existing.Value;
            }
        }

        // Factory outside lock to avoid blocking other threads during value creation.
        var newValue = valueFactory(key);

        lock (_lock)
        {
            if (_store.TryGetValue(key, out var existing))
            {
                _evictionPolicy.RecordAccess(key);
                return existing.Value;
            }

            if (_store.Count >= Capacity && _evictionPolicy.TryEvict(out var evictedKey))
            {
                _store.Remove(evictedKey);
            }

            _store[key] = new CacheEntry(newValue);
            _evictionPolicy.RecordAdd(key);
            return newValue;
        }
    }
}