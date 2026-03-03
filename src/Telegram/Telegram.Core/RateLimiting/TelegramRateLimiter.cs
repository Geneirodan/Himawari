using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace Himawari.Telegram.Core.RateLimiting;

/// <summary>
/// Token bucket rate limiter respecting Telegram Bot API quotas. Global: ~30 msg/s; per-chat: 1 msg/s. Call <see cref="AcquireAsync"/> before sending; dispose the returned lease after send.
/// </summary>
public sealed class TelegramRateLimiter : IDisposable
{
    private readonly RateLimiter _global;
    private readonly ConcurrentDictionary<long, RateLimiter> _perChat = new();
    private bool _disposed;

    /// <summary>
    /// Creates a rate limiter with global (30/s) and per-chat (1/s) token buckets.
    /// </summary>
    public TelegramRateLimiter()
    {
        _global = new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 30,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            TokensPerPeriod = 25,
            QueueLimit = 200,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        });
    }

    /// <summary>
    /// Acquires a send permit for <paramref name="chatId"/>. Awaits until both global and per-chat buckets have tokens. Dispose the result after sending.
    /// </summary>
    /// <param name="chatId">The target chat ID (used for per-chat limit).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A lease that must be disposed after the send. If per-chat lease fails, the global lease is released and an exception is thrown.</returns>
    public async Task<IDisposable> AcquireAsync(long chatId, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var globalLease = await _global.AcquireAsync(1, cancellationToken).ConfigureAwait(false);
        if (!globalLease.IsAcquired)
        {
            globalLease.Dispose();
            throw new TelegramRateLimitExceededException("Global Telegram rate limit exceeded.");
        }

        var perChat = _perChat.GetOrAdd(chatId, _ => new TokenBucketRateLimiter(new TokenBucketRateLimiterOptions
        {
            TokenLimit = 1,
            ReplenishmentPeriod = TimeSpan.FromSeconds(1),
            TokensPerPeriod = 1,
            QueueLimit = 10,
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
        }));

        var chatLease = await perChat.AcquireAsync(1, cancellationToken).ConfigureAwait(false);
        if (!chatLease.IsAcquired)
        {
            globalLease.Dispose();
            throw new TelegramRateLimitExceededException($"Per-chat rate limit exceeded for chat {chatId}.");
        }

        return new CombinedLease(globalLease, chatLease);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
            return;
        _global.Dispose();
        foreach (var limiter in _perChat.Values)
            limiter.Dispose();
        _perChat.Clear();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private sealed class CombinedLease(RateLimitLease global, RateLimitLease perChat) : IDisposable
    {
        public void Dispose()
        {
            global.Dispose();
            perChat.Dispose();
        }
    }
}
