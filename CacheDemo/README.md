# CacheDemo

Simple .NET 8 console app that fetches posts from JSONPlaceholder and demonstrates a high-performance, thread-safe in-memory cache with pluggable eviction (LRU by default).

## Running

```powershell
cd "D:\Sana Assesment\SanaAssesment"
dotnet run --project CacheDemo
```

## Cache design

- `ICache<TKey, TValue>` defines the basic cache surface.
- `IEvictionPolicy<TKey>` allows swapping eviction algorithms.
- `InMemoryCache<TKey, TValue>` is thread-safe, capacity-bounded, and uses the provided eviction policy.
- `LruEvictionPolicy<TKey>` tracks recency with a linked list + dictionary (O(1) for access, add, evict).

## Demo flow

- Cache capacity is set to 3 entries.
- Requests post IDs in the sequence: `1, 2, 1, 3, 2, 4, 1`.
- Outputs whether each fetch is a cache hit or requires a service call.
- Shows final cache utilization after eviction.

## Swapping eviction

Pass a different `IEvictionPolicy<TKey>` into `InMemoryCache` to change eviction behavior. Example:

```csharp
var cache = new InMemoryCache<int, Post>(capacity: 3, evictionPolicy: new MyCustomPolicy());
```

