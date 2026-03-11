using System.Collections.Concurrent;

namespace Himawari.Telegram.Core.Pipeline;

/// <summary>
/// Lock-free keyed semaphore with automatic cleanup of idle keys. Only one thread can hold the lock per key at a time; unused keys are removed to avoid memory leaks.
/// </summary>
/// <typeparam name="TKey">The key type (e.g. <see cref="long"/> for chat ID).</typeparam>
public sealed class KeyedSemaphore<TKey> where TKey : notnull
{
    private readonly ConcurrentDictionary<TKey, RefCountedSemaphore> _locks = new();

    /// <summary>
    /// Waits for exclusive access to the given key. Release with <see cref="Release"/>.
    /// </summary>
    /// <param name="key">The key (e.g. chat ID).</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task WaitAsync(TKey key, CancellationToken ct = default)
    {
        var entry = _locks.AddOrUpdate(
            key,
            _ => new RefCountedSemaphore(),
            (_, existing) =>
            {
                existing.Increment();
                return existing;
            });
        await entry.Semaphore.WaitAsync(ct).ConfigureAwait(false);
    }

    /// <summary>
    /// Releases the lock for the given key. Must be called once per successful <see cref="WaitAsync"/>.
    /// </summary>
    /// <param name="key">The key.</param>
    public void Release(TKey key)
    {
        if (!_locks.TryGetValue(key, out var entry))
            return;
        entry.Semaphore.Release();
        if (entry.Decrement() == 0)
            _locks.TryRemove(key, out _);
    }

    private sealed class RefCountedSemaphore
    {
        public SemaphoreSlim Semaphore { get; } = new(1, 1);
        private int _count = 1;

        public void Increment() => Interlocked.Increment(ref _count);

        public int Decrement() => Interlocked.Decrement(ref _count);
    }
}
