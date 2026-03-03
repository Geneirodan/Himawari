namespace Himawari.Telegram.Core.Options;

/// <summary>
/// Configuration for the Telegram bot (WTelegram): token, API credentials, supported locales, and command-list refresh interval.
/// </summary>
public sealed record BotOptions
{
    /// <summary>Bot token from BotFather.</summary>
    public required string Token { get; init; }
    /// <summary>Telegram API ID (my.telegram.org).</summary>
    public required int ApiId { get; init; }
    /// <summary>Telegram API hash (my.telegram.org).</summary>
    public required string ApiHash { get; init; }
    /// <summary>Locale codes for which to register commands (e.g. "en", "ru").</summary>
    public required string[] SupportedLocales { get; init; }
    /// <summary>Interval after which to refresh the bot command list; use <see cref="TimeSpan.Zero"/> or negative to run once and then delay indefinitely.</summary>
    public required TimeSpan PingTimeout { get; init; }
}