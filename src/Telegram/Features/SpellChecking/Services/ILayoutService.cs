using System.Collections.Frozen;
using WeCantSpell.Hunspell;

namespace Himawari.SpellChecking.Services;

/// <summary>
/// Provides keyboard layout data and Hunspell word lists per locale for wrong-layout detection and spell checking.
/// </summary>
public interface ILayoutService
{
    /// <summary>Gets the Hunspell word list for the given locale.</summary>
    WordList GetWordList(string localeName);
    /// <summary>Gets the character map from wrong layout to correct layout (e.g. en→ru). Built once at startup; O(1) lookup on the hot path.</summary>
    FrozenDictionary<char, char> GetMap(string layoutName);
    /// <summary>Gets the reverse character map (e.g. ru→en). Built once at startup; O(1) lookup on the hot path.</summary>
    FrozenDictionary<char, char> GetReverseMap(string layoutName);
    /// <summary>Returns supported language/locale names.</summary>
    IEnumerable<string> GetSupportedLanguages();
    /// <summary>Returns layout names available for the given locale (excluding the default QWERTY key).</summary>
    IEnumerable<string> GetLayouts(string localeName);
}