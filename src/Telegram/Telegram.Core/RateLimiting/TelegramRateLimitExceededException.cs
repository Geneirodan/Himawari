namespace Himawari.Telegram.Core.RateLimiting;

/// <summary>
/// Thrown when the Telegram send rate limit (global or per-chat) is exceeded.
/// </summary>
public sealed class TelegramRateLimitExceededException : InvalidOperationException
{
    /// <summary>Creates an exception with the given message.</summary>
    public TelegramRateLimitExceededException(string message) : base(message) { }

    /// <summary>Creates an exception with the given message and inner exception.</summary>
    public TelegramRateLimitExceededException(string message, Exception inner) : base(message, inner) { }
}
