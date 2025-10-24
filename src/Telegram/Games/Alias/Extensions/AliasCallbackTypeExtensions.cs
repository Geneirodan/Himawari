using System.Text.RegularExpressions;
using Himawari.Alias.Enums;

namespace Himawari.Alias.Extensions;

public static partial class AliasCallbackTypeExtensions
{
    public static string Serialize(this AliasCallbackType value) => $"{nameof(Alias)}-{value}";

    public static AliasCallbackType Deserialize(this string? value)
    {
        if (value is null)
            return AliasCallbackType.None;
        var str = AliasFormat.Matches(value).FirstOrDefault()?.Groups[1].Value;
        return Enum.TryParse(str, out AliasCallbackType callbackType) ? callbackType : AliasCallbackType.None;
    }

    [GeneratedRegex(@"^Alias-(\w+)$")] private static partial Regex AliasFormat { get; }
}