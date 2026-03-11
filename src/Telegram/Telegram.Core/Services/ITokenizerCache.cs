using JetBrains.Annotations;

namespace Himawari.Telegram.Core.Services;

/// <summary>
/// Caches the result of <see cref="Commands.CommandTokenizer.Tokenize"/> per input string to avoid repeated work.
/// </summary>
[PublicAPI]
public interface ITokenizerCache
{
    /// <summary>
    /// Returns the first token and remainder for the given message text, using cache when available.
    /// </summary>
    /// <param name="messageText">Raw message text.</param>
    /// <returns>First token and remainder; ("", "") when <paramref name="messageText"/> is empty or whitespace.</returns>
    (string FirstToken, string Remainder) Tokenize(string? messageText);
}
