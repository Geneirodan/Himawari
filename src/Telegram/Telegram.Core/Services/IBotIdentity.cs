namespace Himawari.Telegram.Core.Services;

/// <summary>
/// Provides the bot's username (from Telegram GetMe) with lazy caching so that group messages and slash-command parsing do not call the API on every request.
/// </summary>
public interface IBotIdentity
{
    /// <summary>
    /// Returns the bot's @username. Cached after the first successful <c>GetMe</c> call.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The bot username, or <see langword="null"/> if not available.</returns>
    ValueTask<string?> GetUsernameAsync(CancellationToken cancellationToken = default);
}
