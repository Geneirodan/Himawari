namespace Himawari.SpellChecking.Keyboards;

// ReSharper disable file StringLiteralTypo
public class UkrainianKeyboard : Keyboard
{
    protected override Dictionary<char, char> Inner { get; } = Qwerty.Zip(
        "'1234567890-=" +
        "йцукенгшщзхї\\" +
        "фівапролджє" +
        "ячсмитьбю." +
        "₴!\"№;%:?*()_+" +
        "ЙЦУКЕНГШЩЗХЇ|" +
        "ФІВАПРОЛДЖЄ" +
        "ЯЧСМИТЬБЮ,"
    ).ToDictionary(x => x.First, x => x.Second);
}