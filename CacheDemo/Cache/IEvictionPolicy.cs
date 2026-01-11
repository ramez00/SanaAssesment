namespace CacheDemo.Cache;

public interface IEvictionPolicy<TKey>
{
    void RecordAdd(TKey key);
    void RecordAccess(TKey key);
    void RecordRemoval(TKey key);
    bool TryEvict(out TKey key);
}

