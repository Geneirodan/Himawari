namespace Himawari.Alias.Models;

/// <summary>
/// Per-chat session state for the Alias game: category, shuffled words, score, and round timing.
/// </summary>
public sealed class AliasSessionState
{
    /// <summary>Selected category key.</summary>
    public string CategoryKey { get; set; } = string.Empty;

    /// <summary>Culture/language code (e.g. "ru", "uk", "en").</summary>
    public string Culture { get; set; } = "ru";

    /// <summary>Shuffled word list for the current round.</summary>
    public IList<string> ShuffledWords { get; set; } = [];

    /// <summary>Current index in <see cref="ShuffledWords"/>.</summary>
    public int CurrentIndex { get; set; }

    /// <summary>Number of words guessed correctly in this round.</summary>
    public int Score { get; set; }

    /// <summary>Round number (1-based).</summary>
    public int RoundNumber { get; set; } = 1;

    /// <summary>UTC time when the round started.</summary>
    public DateTime RoundStartedAt { get; set; }

    /// <summary>Whether the 60-second round is still active.</summary>
    public bool IsRoundActive => (DateTime.UtcNow - RoundStartedAt).TotalSeconds < 60;
}
