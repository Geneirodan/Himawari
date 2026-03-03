using Himawari.Alias.Models;

namespace Himawari.Alias.Services;

/// <summary>
/// Provides Alias word categories and per-round word sequences with Fisher–Yates shuffle.
/// Words are loaded from <c>AliasGame:Categories</c> configuration.
/// </summary>
public interface IAliasWordService
{
    /// <summary>All configured categories (key, labels, words per language).</summary>
    IReadOnlyList<AliasCategory> GetAllCategories();

    /// <summary>Whether a category with the given key exists.</summary>
    bool HasCategory(string categoryKey);

    /// <summary>
    /// Starts a new round: creates session state with shuffled words for the category and culture.
    /// </summary>
    /// <param name="categoryKey">Category key.</param>
    /// <param name="culture">Culture name (e.g. "ru", "uk-UA", "en"); normalized to "ru" / "uk" / "en".</param>
    /// <returns>New session state with first word index set.</returns>
    AliasSessionState StartRound(string categoryKey, string culture);

    /// <summary>Gets the next word from the session list and advances the index. Reshuffles when exhausted (last word cannot become first).</summary>
    string? GetNextWord(AliasSessionState state);

    /// <summary>Whether there are more words in the current shuffled list for this session.</summary>
    bool HasMoreWords(AliasSessionState state);
}
