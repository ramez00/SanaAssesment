using Xunit;
using Cache_Implementation_Task4.Services;

public class EvictionTests
{
    [Fact]
    public void TryEvict_EmptyPolicy_ReturnsFalse()
    {
        // Arrange
        var policy = new EvictionPolicy<string>();

        // Act
        var result = policy.TryEvict(out var key);

        // Assert
        Assert.False(result);
        Assert.Null(key);
    }

    [Fact]
    public void TryEvict_SingleItem_ReturnsAndRemovesIt()
    {
        // Arrange
        var policy = new EvictionPolicy<string>();
        policy.RecordAdd("key1");

        // Act
        var result = policy.TryEvict(out var key);

        // Assert
        Assert.True(result);
        Assert.Equal("key1", key);

        // Verify it's removed
        var secondResult = policy.TryEvict(out _);
        Assert.False(secondResult);
    }

    [Fact]
    public void TryEvict_MultipleItems_ReturnsLeastRecentlyUsed()
    {
        // Arrange
        var policy = new EvictionPolicy<string>();
        policy.RecordAdd("key1");
        policy.RecordAdd("key2");
        policy.RecordAdd("key3");

        // Act
        var result = policy.TryEvict(out var key);

        // Assert
        Assert.True(result);
        Assert.Equal("key1", key); // First added = LRU
    }

    [Fact]
    public void RecordAccess_MovesItemToEnd()
    {
        // Arrange
        var policy = new EvictionPolicy<string>();
        policy.RecordAdd("key1");
        policy.RecordAdd("key2");
        policy.RecordAdd("key3");

        // Act - Access key1, making it most recently used
        policy.RecordAccess("key1");

        // Assert - key2 should now be LRU
        policy.TryEvict(out var evictedKey);
        Assert.Equal("key2", evictedKey);
    }

    [Fact]
    public void RecordAdd_ExistingKey_MovesToEnd()
    {
        // Arrange
        var policy = new EvictionPolicy<string>();
        policy.RecordAdd("key1");
        policy.RecordAdd("key2");

        // Act - Re-add key1
        policy.RecordAdd("key1");

        // Assert - key2 should be evicted first (now LRU)
        policy.TryEvict(out var key);
        Assert.Equal("key2", key);
    }

    [Fact]
    public void RecordRemoval_RemovesKeyFromPolicy()
    {
        // Arrange
        var policy = new EvictionPolicy<string>();
        policy.RecordAdd("key1");
        policy.RecordAdd("key2");
        policy.RecordAdd("key3");

        // Act
        policy.RecordRemoval("key1");

        // Assert - key2 should be LRU now
        policy.TryEvict(out var key);
        Assert.Equal("key2", key);
    }

    [Fact]
    public void RecordAccess_NonExistentKey_DoesNothing()
    {
        // Arrange
        var policy = new EvictionPolicy<string>();
        policy.RecordAdd("key1");

        // Act
        policy.RecordAccess("nonexistent");

        // Assert - key1 should still be evicted
        policy.TryEvict(out var key);
        Assert.Equal("key1", key);
    }

    [Fact]
    public void ComplexScenario_MaintainsCorrectLRUOrder()
    {
        // Arrange
        var policy = new EvictionPolicy<int>();

        // Act
        policy.RecordAdd(1);
        policy.RecordAdd(2);
        policy.RecordAdd(3);
        policy.RecordAccess(1);  // Order: 2, 3, 1
        policy.RecordAdd(4);     // Order: 2, 3, 1, 4
        policy.RecordAccess(2);  // Order: 3, 1, 4, 2

        // Assert
        policy.TryEvict(out var first);
        Assert.Equal(3, first);

        policy.TryEvict(out var second);
        Assert.Equal(1, second);

        policy.TryEvict(out var third);
        Assert.Equal(expected: 4, third);

        policy.TryEvict(out var fourth);
        Assert.Equal(2, fourth);
    }

    [Fact]
    public void CustomEqualityComparer_WorksCorrectly()
    {
        // Arrange - Case-insensitive comparer
        var policy = new EvictionPolicy<string>(
            StringComparer.OrdinalIgnoreCase);

        policy.RecordAdd("KEY1");
        policy.RecordAdd("key2");

        // Act - Access with different casing
        policy.RecordAccess("key1");

        // Assert
        policy.TryEvict(out var key);
        Assert.Equal("key2", key);
    }
}