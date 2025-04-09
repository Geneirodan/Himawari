using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Himawari.SpellChecking.Extensions;
using WeCantSpell.Hunspell;

namespace Himawari.SpellChecking.Services;

public partial class WrongLayoutParser(ILayoutService service) : IWrongLayoutParser
{
    private const float Threshold = 0.5f;

    public bool TryParse(string inputString, [NotNullWhen(true)] out string? outputString)
    {
        outputString = null;
        var words = WhitespaceRegex.Split(inputString).Where(x => x.Length > 0).ToArray();
        foreach (var lang in service.GetSupportedLanguages())
        {
            var wordList = service.GetWordList(lang);
            foreach (var layout in service.GetLayouts(lang))
                if (TryParse(inputString, out outputString, words, service.GetMap(layout), wordList)
                    || TryParse(inputString, out outputString, words, service.GetReverseMap(layout), wordList))
                    return true;
        }

        return false;
    }

    private static bool TryParse(
        string inputString,
        [NotNullWhen(true)] out string? outputString,
        string[] words,
        IReadOnlyDictionary<char, char> map,
        WordList wordList
    )
    {
        double newHits = words
            .Select(map.Translate)
            .SelectMany(newString => AllowedCharactersRegex.Matches(newString))
            .Select(x => x.Value)
            .Count(wordList.Check);

        if (newHits / words.Length < Threshold)
        {
            outputString = null;
            return false;
        }

        outputString = map.Translate(inputString);
        return outputString != inputString;
    }


    [GeneratedRegex(@"[\s]")] private static partial Regex WhitespaceRegex { get; }

    [GeneratedRegex(@"['\-\w]+")] private static partial Regex AllowedCharactersRegex { get; }
}