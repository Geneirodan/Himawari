using System.Diagnostics.CodeAnalysis;

namespace Himawari.SpellChecking.Services;

/// <summary>
/// Tries to convert text typed in the wrong keyboard layout to the intended text (e.g. "ghbdtn" → "привет" when user meant Russian).
/// </summary>
public interface IWrongLayoutParser
{
    /// <summary>
    /// Attempts to parse <paramref name="inputString"/> as wrong-layout text and output the corrected string.
    /// </summary>
    /// <param name="inputString">Text that may be in the wrong layout.</param>
    /// <param name="outputString">When the method returns <see langword="true"/>, the converted text; otherwise <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if conversion succeeded; otherwise <see langword="false"/>.</returns>
    bool TryParse(string inputString, [NotNullWhen(true)] out string? outputString);
}