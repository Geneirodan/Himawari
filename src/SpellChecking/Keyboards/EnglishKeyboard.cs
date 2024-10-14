using DeepL;

namespace Himawari.SpellChecking.Keyboards;

public class EnglishKeyboard : Keyboard
{
    protected override Dictionary<char, char> Inner { get; } = Qwerty.ToDictionary(x => x, x => x);
}