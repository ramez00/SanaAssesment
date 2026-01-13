using Cache_Implementation_Task4.Interfaces;
using System;
using System.Collections.Generic;

namespace Cache_Implementation_Task4.Services;

/// <summary>
/// Thread-safe, capacity-limited in-memory cache implementation with LRU eviction policy.
/// When the cache reaches capacity, it automatically evicts the least recently used item.
/// </summary>
/// <typeparam name="TKey">The type of keys used to identify cache entries</typeparam>
/// <typeparam name="TValue">The type of values stored in the cache</typeparam>
public class InMemoryCache<TKey, TValue> : ICache<TKey, TValue>
{
    /// <summary>
    /// Internal wrapper for cached values. Using a record provides immutability and clean syntax.
    /// Sealed prevents inheritance for performance optimization.
    /// </summary>
    private sealed record CacheEntry(TValue Value);

    // The actual cache storage: maps keys to their wrapped values
    private readonly Dictionary<TKey, CacheEntry> _store;

    // Manages which items to evict when cache is full (tracks LRU order)
    private readonly IEvictionPolicy<TKey> _evictionPolicy;

    // Lock object for thread synchronization - ensures thread-safe operations
    // All cache operations that modify or read state are protected by this lock
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the InMemoryCache with specified capacity and optional components.
    /// </summary>
    /// <param name="capacity">Maximum number of items the cache can hold (must be > 0)</param>
    /// <param name="evictionPolicy">Optional custom eviction policy (defaults to LRU)</param>
    /// <param name="comparer">Optional custom equality comparer for keys (e.g., case-insensitive)</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when capacity is less than or equal to zero</exception>
    public InMemoryCache(int capacity,
                        IEvictionPolicy<TKey>? evictionPolicy = null,
                        IEqualityComparer<TKey>? comparer = null)
    {
        // Validate capacity - must be positive to make sense
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Capacity must be greater than zero.");

        // Store the capacity as a read-only property
        Capacity = capacity;

        // Initialize the dictionary with the specified capacity and comparer
        // Pre-allocating capacity improves performance by avoiding resizing
        _store = new Dictionary<TKey, CacheEntry>(capacity, comparer);

        // Use provided eviction policy, or create default LRU policy if none provided
        // The ?? operator provides a default value when evictionPolicy is null
        _evictionPolicy = evictionPolicy ?? new EvictionPolicy<TKey>(comparer);
    }

    /// <summary>
    /// Gets the maximum number of items this cache can hold.
    /// This value is set at construction and cannot be changed.
    /// </summary>
    public int Capacity { get; }

    /// <summary>
    /// Gets the current number of items in the cache.
    /// This property is thread-safe.
    /// </summary>
    public int Count
    {
        get
        {
            // Lock to ensure we get a consistent count even if other threads are modifying
            lock (_lock)
            {
                return _store.Count;
            }
        }
    }

    /// <summary>
    /// Attempts to retrieve a value from the cache.
    /// If successful, the item is marked as recently used (moved to end of LRU list).
    /// </summary>
    /// <param name="key">The key to look up</param>
    /// <param name="value">Outputs the cached value if found</param>
    /// <returns>True if the key was found; false otherwise</returns>
    public bool TryGet(TKey key, out TValue value)
    {
        // Lock for thread safety - only one thread can read/write at a time
        lock (_lock)
        {
            // Try to find the entry in the dictionary
            if (_store.TryGetValue(key, out var entry))
            {
                // Found! Record this access so eviction policy knows it was recently used
                // This moves the key to the end of the LRU list (marks as MRU)
                _evictionPolicy.RecordAccess(key);

                // Extract the actual value from the wrapper and return success
                value = entry.Value;
                return true;
            }
        }
        // Not found outside the lock - set output to default and return failure

        // Key not found - set output parameter to default value
        value = default!;  // The ! suppresses nullable warning
        return false;       // Indicate key wasn't found
    }

    /// <summary>
    /// Adds a new entry or updates an existing entry in the cache.
    /// If cache is at capacity and adding a new key, evicts the least recently used item first.
    /// </summary>
    /// <param name="key">The key to add or update</param>
    /// <param name="value">The value to store</param>
    public void Set(TKey key, TValue value)
    {
        // Lock for thread safety - prevents concurrent modifications
        lock (_lock)
        {
            // Check if this is an update to an existing key
            if (_store.ContainsKey(key))
            {
                // UPDATE PATH: Key already exists, just replace the value
                _store[key] = new CacheEntry(value);  // Update with new wrapped value

                // Record as an access to mark it as recently used
                // This moves the key to the end of the LRU list (MRU position)
                _evictionPolicy.RecordAccess(key);

                return;  // Done - no need to check capacity or evict
            }

            // ADD PATH: This is a new key, check if we need to make room

            // Check if cache is at capacity
            if (_store.Count >= Capacity && _evictionPolicy.TryEvict(out var evictedKey))
            {
                // Cache is full - evict the LRU item to make room
                // TryEvict returns the key of the least recently used item
                _store.Remove(evictedKey);  // Remove the evicted entry from storage
            }

            // Add the new entry to the cache
            _store[key] = new CacheEntry(value);  // Store the wrapped value

            // Tell eviction policy about the new key (adds to end of LRU list)
            _evictionPolicy.RecordAdd(key);
        }
    }

    /// <summary>
    /// Gets an existing value or adds a new value using the provided factory function.
    /// This implements the "get-or-create" pattern in a thread-safe manner.
    /// IMPORTANT: The factory function is executed OUTSIDE the lock to avoid blocking other threads.
    /// </summary>
    /// <param name="key">The key to look up or add</param>
    /// <param name="valueFactory">Function to create the value if key doesn't exist</param>
    /// <returns>The existing value if key was found, or the newly created value</returns>
    /// <exception cref="ArgumentNullException">Thrown when valueFactory is null</exception>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        // Validate the factory function - it's required
        if (valueFactory is null)
            throw new ArgumentNullException(nameof(valueFactory));

        // FIRST CHECK: Quick check if key already exists (inside lock)
        lock (_lock)
        {
            // If key exists, return it immediately without calling factory
            if (_store.TryGetValue(key, out var existing))
            {
                // Mark as accessed (move to end of LRU list)
                _evictionPolicy.RecordAccess(key);
                return existing.Value;  // Return the existing value
            }
        }
        // Lock is released here - other threads can now access the cache

        // KEY NOT FOUND: Create the value OUTSIDE the lock
        // This is critical! Factory might be slow (database query, computation, etc.)
        // By executing outside the lock, we don't block other threads from using the cache
        var newValue = valueFactory(key);

        // SECOND CHECK: Re-acquire lock and check again (double-check locking pattern)
        // Why? Another thread might have added this key while we were creating the value
        lock (_lock)
        {
            // Check again if key was added by another thread while we were outside the lock
            if (_store.TryGetValue(key, out var existing))
            {
                // Another thread beat us to it! Discard our new value and use theirs
                _evictionPolicy.RecordAccess(key);
                return existing.Value;  // Return the value that was added by other thread
            }

            // Still doesn't exist - we're the first to add it

            // Check if we need to evict an item to make room
            if (_store.Count >= Capacity && _evictionPolicy.TryEvict(out var evictedKey))
            {
                // Cache is full - evict LRU item
                _store.Remove(evictedKey);
            }

            // Add our newly created value to the cache
            _store[key] = new CacheEntry(newValue);

            // Tell eviction policy about the new key
            _evictionPolicy.RecordAdd(key);

            // Return the value we created
            return newValue;
        }
    }
}