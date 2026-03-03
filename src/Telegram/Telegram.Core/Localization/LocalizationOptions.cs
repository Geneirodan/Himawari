namespace Himawari.Telegram.Core.Localization;

/// <summary>
/// Configures <see cref="LanguageFallbackPolicy"/> via appsettings.json.
/// Section: <see cref="SectionName"/>.
/// </summary>
public sealed class LocalizationOptions
{
    /// <summary>Configuration section key: Telegram:Localization.</summary>
    public const string SectionName = "Telegram:Localization";

    /// <summary>
    /// Overrides or extends the built-in language tag → culture name map.
    /// Key: primary BCP-47 subtag (e.g. "be").
    /// Value: .NET culture name (e.g. "uk-UA") or empty string for English (en).
    /// Example: "be" → "uk-UA" makes Belarusian users get Ukrainian.
    /// </summary>
    public IDictionary<string, string> Mappings { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
}
