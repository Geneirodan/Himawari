using System.Globalization;

namespace Himawari.Telegram.Core.Localization;

/// <summary>
/// Resolves explicit chat language set via /lang.
/// Returns <see langword="null"/> when the chat has no stored preference;
/// the caller should keep the culture set by <see cref="CultureBehavior{TRequest,TResponse}"/> (from LanguageCode).
/// </summary>
public interface IExplicitLanguageResolver
{
    /// <summary>Gets the culture for the chat if an explicit language was set via /lang; otherwise null.</summary>
    /// <param name="chatId">The Telegram chat ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the culture or null.</returns>
    Task<CultureInfo?> ResolveAsync(long chatId, CancellationToken cancellationToken = default);
}
