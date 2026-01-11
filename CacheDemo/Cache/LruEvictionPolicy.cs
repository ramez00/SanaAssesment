using System.Collections.Generic;

namespace CacheDemo.Cache;

public class LruEvictionPolicy<TKey> : IEvictionPolicy<TKey>
{
    private readonly Dictionary<TKey, LinkedListNode<TKey>> _nodes;
    private readonly LinkedList<TKey> _order = new();

    public LruEvictionPolicy(IEqualityComparer<TKey>? comparer = null)
    {
        _nodes = new Dictionary<TKey, LinkedListNode<TKey>>(comparer);
    }

    public void RecordAdd(TKey key)
    {
        if (_nodes.TryGetValue(key, out var existing))
        {
            _order.Remove(existing);
            _order.AddLast(existing);
            return;
        }

        var node = new LinkedListNode<TKey>(key);
        _order.AddLast(node);
        _nodes[key] = node;
    }

    public void RecordAccess(TKey key)
    {
        if (_nodes.TryGetValue(key, out var node))
        {
            _order.Remove(node);
            _order.AddLast(node);
        }
    }

    public void RecordRemoval(TKey key)
    {
        if (_nodes.TryGetValue(key, out var node))
        {
            _order.Remove(node);
            _nodes.Remove(key);
        }
    }

    public bool TryEvict(out TKey key)
    {
        var node = _order.First;
        if (node is null)
        {
            key = default!;
            return false;
        }

        key = node.Value;
        _order.RemoveFirst();
        _nodes.Remove(key);
        return true;
    }
}

