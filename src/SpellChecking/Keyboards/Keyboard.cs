using DeepL;
using DeepL.Model;

namespace Himawari.SpellChecking.Keyboards;

// ReSharper disable file StringLiteralTypo
public abstract class Keyboard
{
    protected abstract Dictionary<char, char> Inner { get; }

    private Dictionary<char, char> Reversed => _reversed ??= Inner.ToDictionary(x => x.Value, x => x.Key);

    private Dictionary<char, char>? _reversed;

    public char GetReversed(char key) => Reversed.GetValueOrDefault(key);

    public char this[char i] => Inner[i];

    protected static readonly IEnumerable<char> Qwerty = "`1234567890-=" +
                                                         "qwertyuiop[]\\" +
                                                         "asdfghjkl;'" +
                                                         "zxcvbnm,./" +
                                                         "~!@#$%^&*()_+" +
                                                         "QWERTYUIOP{}|" +
                                                         "ASDFGHJKL:\"" +
                                                         "ZXCVBNM<>?";

    private static IReadOnlyDictionary<string, Keyboard> Keyboards => new Dictionary<string, Keyboard>
    {
        { LanguageCode.English, new EnglishKeyboard() },
        { LanguageCode.Ukrainian, new UkrainianKeyboard() },
    };

    public static Keyboard GetKeyboard(string language) => Keyboards[language];
}