namespace Himawari.Alias.Options;

/// <summary>
/// Configuration for Alias word sets per category. Binds to the <c>Alias</c> configuration section. Key is category id (e.g. "animals", "movies"); value is the list of words. Use <c>random</c> for remote word source (teoset.com).
/// </summary>
public sealed class AliasWordSetsOptions
{
    /// <summary>Category id → list of words. Empty or missing <c>random</c> means use remote generator.</summary>
    public IDictionary<string, List<string>> WordSets { get; set; } = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
}
