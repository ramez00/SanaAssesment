using Cache_Implementation_Task4.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cache_Implementation_Task4.Services;

/// <summary>
/// Implements the Least Recently Used (LRU) eviction policy for cache management.
/// This policy evicts the least recently accessed item when the cache reaches capacity.
/// </summary>
/// <typeparam name="TKey">The type of keys used to identify cache entries</typeparam>
public class EvictionPolicy<TKey> : IEvictionPolicy<TKey>
{
    // Dictionary for O(1) lookup: maps each key to its corresponding node in the linked list
    // This allows us to quickly find and move nodes without scanning the entire list
    private readonly Dictionary<TKey, LinkedListNode<TKey>> _nodes;

    // Linked list maintaining access order: 
    // - Front (First) = Least Recently Used (LRU) - candidate for eviction
    // - Back (Last) = Most Recently Used (MRU) - most recently accessed
    private readonly LinkedList<TKey> _order = new();

    /// <summary>
    /// Initializes a new instance of the EvictionPolicy with an optional custom equality comparer.
    /// </summary>
    /// <param name="comparer">Optional comparer for key equality (e.g., case-insensitive string comparison)</param>
    public EvictionPolicy(IEqualityComparer<TKey>? comparer = null)
    {
        // Initialize the dictionary with the provided comparer (or default if null)
        _nodes = new Dictionary<TKey, LinkedListNode<TKey>>(comparer);
    }

    /// <summary>
    /// Records when a new key is added to the cache or when an existing key is updated.
    /// Moves the key to the end of the list (marks it as most recently used).
    /// </summary>
    /// <param name="key">The cache key being added or updated</param>
    public void RecordAdd(TKey key)
    {
        // Check if this key already exists in our tracking
        if (_nodes.TryGetValue(key, out var existing))
        {
            // Key exists: move its node to the end (mark as most recently used)
            _order.Remove(existing);      // Remove from current position
            _order.AddLast(existing);     // Add to the end (MRU position)
            return;                        // Exit early, no need to create new node
        }

        // Key is new: create a new node and add it to the end
        var node = new LinkedListNode<TKey>(key);  // Wrap the key in a linked list node
        _order.AddLast(node);                       // Add to end of list (MRU position)
        _nodes[key] = node;                         // Store reference in dictionary for fast lookup
    }

    /// <summary>
    /// Records when a key is accessed (read) from the cache.
    /// Moves the key to the end of the list to mark it as recently used.
    /// </summary>
    /// <param name="key">The cache key being accessed</param>
    public void RecordAccess(TKey key)
    {
        // Try to find the node for this key
        if (_nodes.TryGetValue(key, out var node))
        {
            // Move the node to the end (mark as most recently used)
            _order.Remove(node);      // Remove from current position
            _order.AddLast(node);     // Add to the end (MRU position)
        }
        // If key doesn't exist, do nothing (silent no-op)
        // This can happen if the key was evicted or never added
    }

    /// <summary>
    /// Records when a key is explicitly removed from the cache.
    /// Removes the key from both the linked list and the dictionary.
    /// </summary>
    /// <param name="key">The cache key being removed</param>
    public void RecordRemoval(TKey key)
    {
        // Try to find the node for this key
        if (_nodes.TryGetValue(key, out var node))
        {
            // Remove from both data structures
            _order.Remove(node);      // Remove from linked list (access order tracking)
            _nodes.Remove(key);       // Remove from dictionary (lookup table)
        }
        // If key doesn't exist, do nothing (silent no-op)
    }

    /// <summary>
    /// Attempts to evict (remove) the least recently used key from the policy.
    /// This is called when the cache reaches capacity and needs to make room for a new entry.
    /// </summary>
    /// <param name="key">Outputs the evicted key if successful</param>
    /// <returns>True if a key was evicted; false if the policy is empty</returns>
    public bool TryEvict(out TKey key)
    {
        // Get the first node (front of list) - this is the Least Recently Used (LRU)
        var node = _order.First;

        // Check if the list is empty
        if (node is null)
        {
            // No items to evict - return default value and false
            key = default!;           // Set output parameter to default
            return false;              // Indicate failure - nothing to evict
        }

        // Extract the key from the LRU node
        key = node.Value;

        // Remove from both data structures
        _order.RemoveFirst();         // Remove first node from linked list (O(1) operation)
        _nodes.Remove(key);           // Remove from dictionary (O(1) operation)

        return true;                   // Indicate success - key was evicted
    }
}