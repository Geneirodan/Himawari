using System.Text;

namespace Himawari.SpellChecking.Extensions;

public static class ReadOnlyDictionaryExtensions
{
    public static string Translate(this IReadOnlyDictionary<char, char> map, string word)
    {
        return word.Aggregate(
            new StringBuilder(),
            (sb, c) => sb.Append(map.GetValueOrDefault(c, c)),
            sb => sb.ToString()
        );
    }
}