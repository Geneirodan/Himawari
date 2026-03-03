namespace Himawari.Alias.Models;

/// <summary>
/// A category of words for the Alias game with localized labels and word lists per language.
/// </summary>
/// <param name="Key">Category key (e.g. "things", "people").</param>
/// <param name="Labels">Language code (e.g. "ru", "uk", "en") to display label.</param>
/// <param name="Words">Language code to list of words for that language.</param>
public sealed record AliasCategory(
    string Key,
    IReadOnlyDictionary<string, string> Labels,
    IReadOnlyDictionary<string, List<string>> Words);
