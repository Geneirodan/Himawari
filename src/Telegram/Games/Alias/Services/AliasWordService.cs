using Himawari.Alias.Models;
using Microsoft.Extensions.Configuration;

namespace Himawari.Alias.Services;

/// <summary>
/// Loads Alias categories and words from <c>AliasGame:Categories</c> configuration and provides shuffled word sequences using Fisher–Yates. Reshuffles when exhausted; last word of a round cannot become first of the next.
/// </summary>
public sealed class AliasWordService : IAliasWordService
{
    private readonly IReadOnlyList<AliasCategory> _categories;
    private readonly Random _rng = Random.Shared;

    /// <summary>Builds categories from configuration.</summary>
    /// <param name="configuration">Application configuration; reads <c>AliasGame:Categories</c>.</param>
    public AliasWordService(IConfiguration configuration)
    {
        var section = configuration.GetSection("AliasGame:Categories");
        var list = new List<AliasCategory>();
        foreach (var child in section.GetChildren())
        {
            var key = child.Key;
            var labelsSection = child.GetSection("Labels");
            var labels = labelsSection.GetChildren().ToDictionary(x => x.Key, x => x.Value ?? "", StringComparer.OrdinalIgnoreCase);
            var wordsSection = child.GetSection("Words");
            var words = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            foreach (var langSection in wordsSection.GetChildren())
            {
                var wordsList = langSection.GetChildren().Select(w => w.Value ?? "").Where(s => s.Length > 0).ToList();
                if (wordsList.Count > 0)
                    words[langSection.Key] = wordsList;
            }
            if (labels.Count > 0 && words.Count > 0)
                list.Add(new AliasCategory(key, labels, words));
        }
        _categories = list;
    }

    /// <inheritdoc />
    public IReadOnlyList<AliasCategory> GetAllCategories() => _categories;

    /// <inheritdoc />
    public bool HasCategory(string categoryKey) =>
        _categories.Any(c => string.Equals(c.Key, categoryKey, StringComparison.OrdinalIgnoreCase));

    /// <inheritdoc />
    public AliasSessionState StartRound(string categoryKey, string culture)
    {
        var cat = _categories.First(c => string.Equals(c.Key, categoryKey, StringComparison.OrdinalIgnoreCase));
        var lang = NormalizeLang(culture);
        if (!cat.Words.TryGetValue(lang, out var list) || list.Count == 0)
            lang = cat.Words.Keys.First();
        list = cat.Words[lang];
        var shuffled = new List<string>(list);
        FisherYatesShuffle(shuffled, excludeFirst: null);
        return new AliasSessionState
        {
            CategoryKey = categoryKey,
            Culture = lang,
            ShuffledWords = shuffled,
            CurrentIndex = 0,
            Score = 0,
            RoundNumber = 1,
            RoundStartedAt = DateTime.UtcNow
        };
    }

    /// <inheritdoc />
    public string? GetNextWord(AliasSessionState state)
    {
        var words = state.ShuffledWords;
        if (words.Count == 0) return null;

        state.CurrentIndex++;
        if (state.CurrentIndex >= words.Count)
        {
            var lastWord = words[^1];
            FisherYatesShuffle(words, excludeFirst: lastWord);
            state.CurrentIndex = 0;
        }
        return words[state.CurrentIndex];
    }

    /// <inheritdoc />
    public bool HasMoreWords(AliasSessionState state) => state.ShuffledWords.Count > 0;

    private static string NormalizeLang(string culture)
    {
        if (string.IsNullOrEmpty(culture)) return "ru";
        var c = culture.AsSpan().Trim();
        if (c.StartsWith("uk", StringComparison.OrdinalIgnoreCase)) return "uk";
        if (c.StartsWith("en", StringComparison.OrdinalIgnoreCase)) return "en";
        return "ru";
    }

    private void FisherYatesShuffle(IList<string> list, string? excludeFirst)
    {
        for (var i = list.Count - 1; i > 0; i--)
        {
            var j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
        if (excludeFirst is { } ex && list.Count > 1 && string.Equals(list[0], ex, StringComparison.Ordinal))
        {
            var swap = _rng.Next(1, list.Count);
            (list[0], list[swap]) = (list[swap], list[0]);
        }
    }
}
