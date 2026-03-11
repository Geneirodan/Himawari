using System.Diagnostics.CodeAnalysis;
using Himawari.Telegram.Core.Abstractions;

namespace Himawari.SpellChecking.Services;

/// <summary>
/// Corrects command tokens typed in the wrong keyboard layout by delegating to <see cref="IWrongLayoutParser"/>.
/// Registered when SpellChecking is enabled so that slash and natural-language commands (e.g. /руды → /help) are resolved.
/// </summary>
public sealed class WrongLayoutCommandCorrector(IWrongLayoutParser parser) : ICommandLayoutCorrector
{
    /// <inheritdoc />
    public bool TryCorrect(string input, [NotNullWhen(true)] out string? output) => parser.TryParse(input, out output);
}
