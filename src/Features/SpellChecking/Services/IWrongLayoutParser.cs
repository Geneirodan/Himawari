using System.Diagnostics.CodeAnalysis;

namespace Himawari.SpellChecking.Services;

public interface IWrongLayoutParser
{
    bool TryParse(string inputString, [NotNullWhen(true)] out string? outputString);
}