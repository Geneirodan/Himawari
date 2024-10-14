using System.Text;
using Himawari.SpellChecking.Keyboards;

namespace Himawari.SpellChecking.Extensions;

public static class StringExtensions
{
    public static string Translate(this string @string, Keyboard keyboard)
    {
        return @string.Aggregate(
            new StringBuilder(),
            (sb, c) => sb.Append(char.IsWhiteSpace(c) ? c : keyboard[c]),
            sb => sb.ToString());
    }
    
    public static string TranslateReversed(this string @string, Keyboard keyboard)
    {
        return @string.Aggregate(
            new StringBuilder(),
            (sb, c) => sb.Append(char.IsWhiteSpace(c) ? c :keyboard.GetReversed(c)),
            sb => sb.ToString());
    }
}