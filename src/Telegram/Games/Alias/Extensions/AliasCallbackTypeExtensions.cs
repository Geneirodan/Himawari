using System.Text.RegularExpressions;
using Himawari.Alias.Enums;

namespace Himawari.Alias.Extensions;

/// <summary>
/// Serializes <see cref="AliasCallbackType"/> to/from callback data strings (e.g. "Alias-Choose", "Alias-NextWord") for inline buttons.
/// </summary>
public static partial class AliasCallbackTypeExtensions
{
    /// <summary>Returns the callback data string for the given type (e.g. "Alias-Choose").</summary>
    public static string Serialize(this AliasCallbackType value) => $"{nameof(Alias)}-{value}";

    /// <summary>Parses callback data back to <see cref="AliasCallbackType"/>; returns <see cref="AliasCallbackType.None"/> if invalid or null.</summary>
    public static AliasCallbackType Deserialize(this string? value)
    {
        if (value is null)
            return AliasCallbackType.None;
        var str = AliasFormat.Matches(value).FirstOrDefault()?.Groups[1].Value;
        return Enum.TryParse(str, out AliasCallbackType callbackType) ? callbackType : AliasCallbackType.None;
    }

    [GeneratedRegex(@"^Alias-(\w+)$")] private static partial Regex AliasFormat { get; }
}