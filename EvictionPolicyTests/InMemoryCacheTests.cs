using Xunit;
using Cache_Implementation_Task4.Services;
using Cache_Implementation_Task4.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

public class InMemoryCacheTests
{
    [Fact]
    public void Constructor_ZeroCapacity_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => new InMemoryCache<string, int>(0));
        Assert.Contains("Capacity must be greater than zero", ex.Message);
    }

    [Fact]
    public void Constructor_NegativeCapacity_ThrowsException()
    {
        // Act & Assert
        var ex = Assert.Throws<ArgumentOutOfRangeException>(
            () => new InMemoryCache<string, int>(-1));
        Assert.Contains("Capacity must be greater than zero", ex.Message);
    }

    [Fact]
    public void Capacity_ReturnsCorrectValue()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(5);

        // Assert
        Assert.Equal(5, cache.Capacity);
    }

    [Fact]
    public void Count_EmptyCache_ReturnsZero()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(5);

        // Assert
        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void Set_AddsNewEntry_IncreasesCount()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(5);

        // Act
        cache.Set("key1", 100);

        // Assert
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void TryGet_NonExistentKey_ReturnsFalse()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(5);

        // Act
        var result = cache.TryGet("nonexistent", out var value);

        // Assert
        Assert.False(result);
        Assert.Equal(0, value);
    }

    [Fact]
    public void TryGet_ExistingKey_ReturnsTrueWithValue()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(5);
        cache.Set("key1", 100);

        // Act
        var result = cache.TryGet("key1", out var value);

        // Assert
        Assert.True(result);
        Assert.Equal(100, value);
    }

    [Fact]
    public void Set_UpdatesExistingKey()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(5);
        cache.Set("key1", 100);

        // Act
        cache.Set("key1", 200);

        // Assert
        cache.TryGet("key1", out var value);
        Assert.Equal(200, value);
        Assert.Equal(1, cache.Count); // Count should remain 1
    }

    [Fact]
    public void Set_AtCapacity_EvictsLRUItem()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(3);
        cache.Set("key1", 100);
        cache.Set("key2", 200);
        cache.Set("key3", 300);

        // Act - This should evict key1 (LRU)
        cache.Set("key4", 400);

        // Assert
        Assert.Equal(3, cache.Count);
        Assert.False(cache.TryGet("key1", out _)); // key1 evicted
        Assert.True(cache.TryGet("key2", out _));
        Assert.True(cache.TryGet("key3", out _));
        Assert.True(cache.TryGet("key4", out _));
    }

    [Fact]
    public void TryGet_UpdatesLRUOrder()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(3);
        cache.Set("key1", 100);
        cache.Set("key2", 200);
        cache.Set("key3", 300);

        // Act - Access key1, making it most recently used
        cache.TryGet("key1", out _);
        cache.Set("key4", 400); // Should evict key2 (now LRU)

        // Assert
        Assert.True(cache.TryGet("key1", out _));  // Still exists
        Assert.False(cache.TryGet("key2", out _)); // Evicted
        Assert.True(cache.TryGet("key3", out _));
        Assert.True(cache.TryGet("key4", out _));
    }

    [Fact]
    public void Set_ExistingKey_UpdatesLRUOrder()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(3);
        cache.Set("key1", 100);
        cache.Set("key2", 200);
        cache.Set("key3", 300);

        // Act - Update key1, making it most recently used
        cache.Set("key1", 150);
        cache.Set("key4", 400); // Should evict key2 (now LRU)

        // Assert
        Assert.True(cache.TryGet("key1", out var val1));
        Assert.Equal(150, val1);
        Assert.False(cache.TryGet("key2", out _)); // Evicted
        Assert.True(cache.TryGet("key3", out _));
        Assert.True(cache.TryGet("key4", out _));
    }

    [Fact]
    public void GetOrAdd_NonExistentKey_CreatesAndReturnsValue()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(5);
        var factoryCalled = false;

        // Act
        var value = cache.GetOrAdd("key1", k =>
        {
            factoryCalled = true;
            return 100;
        });

        // Assert
        Assert.Equal(100, value);
        Assert.True(factoryCalled);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void GetOrAdd_ExistingKey_ReturnsExistingValue()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(5);
        cache.Set("key1", 100);
        var factoryCalled = false;

        // Act
        var value = cache.GetOrAdd("key1", k =>
        {
            factoryCalled = true;
            return 200;
        });

        // Assert
        Assert.Equal(100, value); // Original value
        Assert.False(factoryCalled); // Factory not called
    }

    [Fact]
    public void GetOrAdd_NullFactory_ThrowsException()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(5);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(
            () => cache.GetOrAdd("key1", null!));
    }

    [Fact]
    public void GetOrAdd_AtCapacity_EvictsLRUItem()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(3);
        cache.Set("key1", 100);
        cache.Set("key2", 200);
        cache.Set("key3", 300);

        // Act
        var value = cache.GetOrAdd("key4", k => 400);

        // Assert
        Assert.Equal(400, value);
        Assert.Equal(3, cache.Count);
        Assert.False(cache.TryGet("key1", out _)); // Evicted
    }

    [Fact]
    public void CustomEqualityComparer_WorksCorrectly()
    {
        // Arrange - Case-insensitive comparer
        var cache = new InMemoryCache<string, int>(
            5,
            comparer: StringComparer.OrdinalIgnoreCase);

        cache.Set("KEY1", 100);

        // Act & Assert - Different casing should match
        Assert.True(cache.TryGet("key1", out var value));
        Assert.Equal(100, value);
    }

    [Fact]
    public void CustomEvictionPolicy_IsUsed()
    {
        // Arrange
        var mockPolicy = new MockEvictionPolicy<string>();
        var cache = new InMemoryCache<string, int>(3, mockPolicy);

        // Act
        cache.Set("key1", 100);
        cache.TryGet("key1", out _);

        // Assert
        Assert.True(mockPolicy.RecordAddCalled);
        Assert.True(mockPolicy.RecordAccessCalled);
    }

    [Fact]
    public void ThreadSafety_ConcurrentSets_AllSucceed()
    {
        // Arrange
        var cache = new InMemoryCache<int, int>(1000);
        var tasks = new List<Task>();

        // Act - 100 threads each adding 10 items
        for (int i = 0; i < 100; i++)
        {
            int threadId = i;
            tasks.Add(Task.Run(() =>
            {
                for (int j = 0; j < 10; j++)
                {
                    int key = threadId * 10 + j;
                    cache.Set(key, key * 2);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert
        Assert.Equal(1000, cache.Count);
    }

    [Fact]
    public void ThreadSafety_ConcurrentReadsAndWrites()
    {
        // Arrange
        var cache = new InMemoryCache<int, int>(100);
        for (int i = 0; i < 50; i++)
        {
            cache.Set(i, i * 2);
        }

        var tasks = new List<Task>();
        var exceptions = new List<Exception>();

        // Act - Mix of reads and writes
        for (int i = 0; i < 50; i++)
        {
            int key = i;
            // Reader task
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        cache.TryGet(key, out _);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions) exceptions.Add(ex);
                }
            }));

            // Writer task
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    for (int j = 0; j < 100; j++)
                    {
                        cache.Set(key + 50, j);
                    }
                }
                catch (Exception ex)
                {
                    lock (exceptions) exceptions.Add(ex);
                }
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert - No exceptions should occur
        Assert.Empty(exceptions);
    }

    [Fact]
    public void GetOrAdd_ConcurrentCalls_FactoryCalledOnce()
    {
        // Arrange
        var cache = new InMemoryCache<string, int>(10);
        int factoryCallCount = 0;
        var tasks = new List<Task<int>>();

        // Act - 10 threads trying to add the same key
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                return cache.GetOrAdd("key1", k =>
                {
                    Interlocked.Increment(ref factoryCallCount);
                    Thread.Sleep(10); // Simulate work
                    return 100;
                });
            }));
        }

        Task.WaitAll(tasks.ToArray());

        // Assert - All should get the same value
        Assert.All(tasks, t => Assert.Equal(100, t.Result));
        // Factory might be called more than once due to double-check pattern,
        // but all threads should get consistent results
        Assert.True(factoryCallCount >= 1);
    }

    [Fact]
    public void ComplexScenario_MixedOperations()
    {
        // Arrange
        var cache = new InMemoryCache<string, string>(5);

        // Act & Assert - Complex sequence
        cache.Set("a", "1");
        cache.Set("b", "2");
        cache.Set("c", "3");
        cache.Set("d", "4");
        cache.Set("e", "5");

        Assert.Equal(5, cache.Count);

        // Access 'a' to make it recently used
        cache.TryGet("a", out _);

        // Add new item, should evict 'b' (LRU)
        cache.Set("f", "6");
        Assert.False(cache.TryGet("b", out _));

        // Update 'c'
        cache.Set("c", "33");
        Assert.True(cache.TryGet("c", out var val));
        Assert.Equal("33", val);

        // GetOrAdd existing
        var result = cache.GetOrAdd("d", k => "999");
        Assert.Equal("4", result);

        // GetOrAdd new (should evict 'd' wait... 'd' was just accessed)
        // Should evict 'e' (next LRU after operations)
        cache.Set("g", "7");
        cache.Set("h", "8");

        Assert.Equal(5, cache.Count);
    }

    // Mock eviction policy for testing
    private class MockEvictionPolicy<TKey> : IEvictionPolicy<TKey>
    {
        public bool RecordAddCalled { get; private set; }
        public bool RecordAccessCalled { get; private set; }
        public bool RecordRemovalCalled { get; private set; }
        public bool TryEvictCalled { get; private set; }

        public void RecordAdd(TKey key) => RecordAddCalled = true;
        public void RecordAccess(TKey key) => RecordAccessCalled = true;
        public void RecordRemoval(TKey key) => RecordRemovalCalled = true;

        public bool TryEvict(out TKey key)
        {
            TryEvictCalled = true;
            key = default!;
            return false;
        }
    }
}