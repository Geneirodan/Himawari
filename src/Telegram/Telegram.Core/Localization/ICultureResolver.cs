using System.Globalization;

namespace Himawari.Telegram.Core.Localization;

/// <summary>
/// Resolves a <see cref="CultureInfo"/> from a Telegram BCP-47
/// <c>LanguageCode</c> (e.g. "ru", "uk-UA", "be").
/// Implementations must be thread-safe (registered as Singleton).
/// </summary>
public interface ICultureResolver
{
    /// <summary>
    /// Returns the best-match supported culture.
    /// Never throws; falls back to a default English culture (e.g. en) when unsupported or null.
    /// </summary>
    /// <param name="languageCode">Telegram user language code (BCP-47).</param>
    /// <returns>The resolved culture for localization.</returns>
    CultureInfo Resolve(string? languageCode);
}
