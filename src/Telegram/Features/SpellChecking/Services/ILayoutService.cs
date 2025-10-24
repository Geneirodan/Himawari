using WeCantSpell.Hunspell;

namespace Himawari.SpellChecking.Services;

public interface ILayoutService
{
    WordList GetWordList(string localeName);
    IReadOnlyDictionary<char, char> GetMap(string localeName);
    IReadOnlyDictionary<char, char> GetReverseMap(string localeName);
    IEnumerable<string> GetSupportedLanguages();
    IEnumerable<string> GetLayouts(string localeName);
}