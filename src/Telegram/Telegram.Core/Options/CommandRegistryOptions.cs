namespace Himawari.Telegram.Core.Options;

/// <summary>
/// Options for the command registry: fuzzy (typo) matching, "Did you mean?" suggestions,
/// and optional natural-language triggers (phrases without leading slash that map to commands).
/// Bound from <c>Telegram:Commands</c>; nested keys like ShutUp are ignored.
/// </summary>
public sealed class CommandRegistryOptions
{
    /// <summary>Maximum Levenshtein distance for fuzzy suggestions (default 2).</summary>
    public int FuzzyMaxDistance { get; set; } = 2;

    /// <summary>Whether to send "Did you mean /help?" when fuzzy match finds suggestions (default <see langword="true"/>).</summary>
    public bool ShowSuggestions { get; set; } = true;

    /// <summary>
    /// Optional map from regex pattern to canonical command name (without slash).
    /// When a message does not start with '/', the trimmed text is matched against each key as a regex (first match wins).
    /// Use <c>(?i)</c> for case-insensitivity and <c>\b</c> for word boundaries so that e.g. "who" does not match "whoever".
    /// Example: <c>"(?i)\b(help|хелп|помощь)\b"</c> → "help".
    /// </summary>
    public IDictionary<string, string>? NaturalLanguageTriggers { get; set; }

    /// <summary>
    /// When <see langword="true"/>, natural-language triggers are only matched in private chats.
    /// When <see langword="false"/> (Variant B), NL also works in groups when the message starts with @botname (e.g. "@MyBot хелп").
    /// </summary>
    public bool NaturalLanguageInPrivateChatsOnly { get; set; } = false;
}
