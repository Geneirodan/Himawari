using System.Diagnostics.CodeAnalysis;

namespace Himawari.Telegram.Core.Abstractions;

/// <summary>
/// Attempts to correct a command token that may have been typed in the wrong keyboard layout
/// (e.g. "руды" when the user meant "help" on a Russian layout). Used before <see cref="Commands.CommandRegistry.Resolve"/>.
/// </summary>
/// <remarks>
/// When SpellChecking is enabled, the host can register an implementation (e.g. delegating to a wrong-layout
/// parser) so that slash and natural-language commands are corrected before lookup.
/// </remarks>
public interface ICommandLayoutCorrector
{
    /// <summary>
    /// Tries to convert <paramref name="input"/> from wrong-layout text to the intended command token.
    /// </summary>
    /// <param name="input">Raw command token (e.g. from slash or first NL word).</param>
    /// <param name="output">When the method returns <see langword="true"/>, the corrected token; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if conversion succeeded; otherwise <see langword="false"/>.</returns>
    bool TryCorrect(string input, [NotNullWhen(true)] out string? output);
}
