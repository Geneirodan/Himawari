using Himawari.SpellChecking.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WeCantSpell.Hunspell;
using YamlDotNet.Serialization;

namespace Himawari.SpellChecking.Services;

using Maps = Dictionary<string, Dictionary<char, char>>;

public sealed partial class LayoutService : ILayoutService
{
    private const string DefaultLayoutKey = "qwerty";

    private readonly LayoutSettings _layoutSettings;
    private readonly ILogger<LayoutService> _logger;
    private readonly Maps _maps;
    private readonly Maps _reversedMaps;
    private readonly Dictionary<string, WordList> _wordLists;

    public LayoutService(
        IDeserializer deserializer,
        IOptions<SpellCheckingOptions> options,
        ILogger<LayoutService> logger
    )
    {
        _logger = logger;
        var spellcheckingOptions = options.Value;

        using (var streamReader = new StreamReader(spellcheckingOptions.LayoutsFilePath))
        {
            _layoutSettings = deserializer.Deserialize<LayoutSettings>(streamReader);
        }

        _wordLists = FillWordLists(spellcheckingOptions);
        (_maps, _reversedMaps) = FillMaps();
    }

    public WordList GetWordList(string localeName)
    {
        return _wordLists[localeName];
    }

    public IReadOnlyDictionary<char, char> GetMap(string localeName)
    {
        return _maps[localeName];
    }

    public IReadOnlyDictionary<char, char> GetReverseMap(string localeName)
    {
        return _reversedMaps[localeName];
    }

    public IEnumerable<string> GetSupportedLanguages()
    {
        return _layoutSettings.Locales.Keys;
    }

    public IEnumerable<string> GetLayouts(string localeName)
    {
        return _layoutSettings.Locales[localeName].Where(x => !string.Equals(x, DefaultLayoutKey, StringComparison.OrdinalIgnoreCase));
    }

    private Dictionary<string, WordList> FillWordLists(SpellCheckingOptions spellCheckingOptions)
    {
        var dictionariesFolder = Path.GetFullPath(spellCheckingOptions.DictionariesPath);
        LogDictionariesFolder(dictionariesFolder);

        var affixFolder = spellCheckingOptions.AccPath is null
            ? dictionariesFolder
            : Path.GetFullPath(spellCheckingOptions.AccPath);
        LogAffixesFolder(affixFolder);

        return _layoutSettings.Locales.Keys.ToDictionary(x => x, x =>
        {
            var dictionaryFilePath = Path.Combine(dictionariesFolder, $"{x}.dic");
            var affixFilePath = Path.Combine(affixFolder, $"{x}.aff");
            return WordList.CreateFromFiles(dictionaryFilePath, affixFilePath);
        }, StringComparer.OrdinalIgnoreCase);
    }

    private (Maps maps, Maps reversedMaps) FillMaps()
    {
        if (!_layoutSettings.Layouts.TryGetValue(DefaultLayoutKey, out var qwerty))
            throw new InvalidOperationException("QWERTY layout is necessary for spellchecking");

        var standardQwerty = qwerty.Standard.SelectMany(x => x).ToArray();
        var shiftQwerty = qwerty.Shift.SelectMany(x => x).ToArray();
        var fullQwerty = standardQwerty.Union(shiftQwerty).ToArray();

        var maps = new Maps(StringComparer.OrdinalIgnoreCase);
        var reversedMaps = new Maps(StringComparer.OrdinalIgnoreCase);
        var layouts = _layoutSettings.Layouts.Where(x => !string.Equals(x.Key, DefaultLayoutKey, StringComparison.OrdinalIgnoreCase));
        foreach (var (key, keyboardLayout) in layouts)
        {
            var standard = keyboardLayout.Standard.SelectMany(x => x);
            var shift = keyboardLayout.Shift.SelectMany(x => x);
            var zipped = standard.Union(shift).Zip(fullQwerty).ToArray();
            maps.Add(key, zipped.ToDictionary(x => x.First, x => x.Second));
            reversedMaps.Add(key, zipped.ToDictionary(x => x.Second, x => x.First));
        }

        return (maps, reversedMaps);
    }

    [LoggerMessage(LogLevel.Information, "Dictionaries folder: {Path}")]
    private partial void LogDictionariesFolder(string path);

    [LoggerMessage(LogLevel.Information, "Affixes folder: {Path}")]
    private partial void LogAffixesFolder(string path);
}