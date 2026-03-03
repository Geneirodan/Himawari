using System.Text;

namespace Himawari.SpellChecking.Extensions;

/// <summary>Extension methods for character maps used in wrong-layout conversion.</summary>
public static class ReadOnlyDictionaryExtensions
{
    /// <summary>Translates each character using the map; characters not in the map are left unchanged.</summary>
    /// <param name="map">Character-to-character map (e.g. wrong layout → correct layout).</param>
    /// <param name="word">Input string to translate.</param>
    /// <returns>The translated string.</returns>
    public static string Translate(this IReadOnlyDictionary<char, char> map, string word)
    {
        return word.Aggregate(
            new StringBuilder(),
            (sb, c) => sb.Append(map.GetValueOrDefault(c, c)),
            sb => sb.ToString()
        );
    }
}