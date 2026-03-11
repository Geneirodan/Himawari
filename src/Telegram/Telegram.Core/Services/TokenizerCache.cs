using System.Collections.Generic;
using Himawari.Telegram.Core.Commands;
using JetBrains.Annotations;

namespace Himawari.Telegram.Core.Services;

/// <summary>
/// Thread-safe LRU cache for <see cref="CommandTokenizer.Tokenize"/> results.
/// Evicts the least-recently-used entry when capacity is reached.
/// Uses <see cref="LinkedList{T}"/> + <see cref="Dictionary{TKey,TValue}"/> for O(1) get and eviction.
/// </summary>
[PublicAPI]
public sealed class TokenizerCache : ITokenizerCache
{
    /// <summary>Default maximum number of entries to cache.</summary>
    public const int DefaultCapacity = 1024;

    private readonly int _capacity;
    private readonly Dictionary<string, LinkedListNode<CacheEntry>> _map;
    private readonly LinkedList<CacheEntry> _order;
    private readonly Lock _lock = new();

    /// <summary>
    /// Creates an LRU tokenizer cache with the given capacity.
    /// </summary>
    /// <param name="capacity">Maximum number of entries to cache (default 1024).</param>
    public TokenizerCache(int capacity = DefaultCapacity)
    {
        _capacity = capacity > 0 ? capacity : DefaultCapacity;
        _map = new Dictionary<string, LinkedListNode<CacheEntry>>(_capacity, StringComparer.Ordinal);
        _order = new LinkedList<CacheEntry>();
    }

    /// <inheritdoc />
    public (string FirstToken, string Remainder) Tokenize(string? messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
            return (string.Empty, string.Empty);

        lock (_lock)
        {
            if (_map.TryGetValue(messageText, out var node))
            {
                _order.Remove(node);
                _order.AddFirst(node);
                return (node.Value.FirstToken, node.Value.Remainder);
            }
        }

        var result = CommandTokenizer.Tokenize(messageText);

        lock (_lock)
        {
            if (_map.TryGetValue(messageText, out var existing))
                return (existing.Value.FirstToken, existing.Value.Remainder);

            if (_map.Count >= _capacity)
                Evict();

            var entry = new CacheEntry(messageText, result.FirstToken, result.Remainder);
            var newNode = _order.AddFirst(entry);
            _map[messageText] = newNode;
        }

        return result;
    }

    /// <summary>Removes the least-recently-used (tail) entry.</summary>
    private void Evict()
    {
        var lru = _order.Last;
        if (lru is null)
            return;
        _order.RemoveLast();
        _map.Remove(lru.Value.Key);
    }

    private readonly record struct CacheEntry(string Key, string FirstToken, string Remainder);
}
