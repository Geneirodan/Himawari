using JetBrains.Annotations;

namespace Himawari.Telegram.Core.Commands;

/// <summary>
/// Splits message text into the first token (word) and the remainder, for use in slash-command parsing
/// and natural-language trigger lookup (e.g. "help me" → token "help", rest "me").
/// </summary>
[PublicAPI]
public static class CommandTokenizer
{
    /// <summary>
    /// Tokenizes the message text: trims and splits on the first run of whitespace.
    /// </summary>
    /// <param name="messageText">Raw message text (e.g. "  help   me  " or "помоги пожалуйста").</param>
    /// <returns>
    /// First token (first non-empty word) and the remainder of the text after the first whitespace run.
    /// If <paramref name="messageText"/> is empty or whitespace, returns ("", "").
    /// </returns>
    /// <remarks>
    /// Use this for natural-language commands (no leading slash): the first token can be matched
    /// against <see cref="CommandRegistryOptions.NaturalLanguageTriggers"/> and the remainder passed as arguments.
    /// </remarks>
    public static (string FirstToken, string Remainder) Tokenize(string? messageText)
    {
        if (string.IsNullOrWhiteSpace(messageText))
            return (string.Empty, string.Empty);

        var span = messageText.AsSpan().Trim();
        var spaceIndex = span.IndexOfAny("\t\n\r "); // any whitespace
        if (spaceIndex < 0)
            return (span.ToString(), string.Empty);

        var first = span[..spaceIndex].Trim().ToString();
        var remainder = span[(spaceIndex + 1)..].Trim().ToString();
        return (first, remainder);
    }
}
