namespace Himawari.Telegram.Core.Localization;

/// <summary>
/// Persists explicit language preferences set via /lang.
/// Implementation is responsible for caching.
/// </summary>
public interface ILanguageRepository
{
    /// <summary>Returns stored language code or <see langword="null"/> if not set.</summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the language code or null.</returns>
    Task<string?> GetAsync(long chatId, CancellationToken cancellationToken = default);

    /// <summary>Persists language code for the chat.</summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="languageCode">BCP-47 primary subtag (e.g. "en", "uk").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task SetAsync(long chatId, string languageCode, CancellationToken cancellationToken = default);
}
