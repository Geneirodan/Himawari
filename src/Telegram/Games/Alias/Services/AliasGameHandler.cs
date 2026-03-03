using Himawari.Alias.Models;

namespace Himawari.Alias.Services;

/// <summary>
/// Manages per-chat Alias game sessions and 60-second rounds. Words come from <see cref="IAliasWordService"/>; score and round state are tracked in memory. For horizontal scaling, replace in-memory sessions with <see cref="IDistributedCache"/> or Redis.
/// </summary>
public sealed class AliasGameHandler
{
    private readonly IAliasWordService _wordService;
    private readonly Dictionary<long, AliasSessionState> _sessions = [];

    /// <summary>Creates the handler with the word service.</summary>
    /// <param name="wordService">Provides categories and shuffled words.</param>
    public AliasGameHandler(IAliasWordService wordService)
    {
        _wordService = wordService;
    }

    /// <summary>Starts a round for the chat: creates session and returns the first word.</summary>
    /// <param name="chatId">Telegram chat ID.</param>
    /// <param name="categoryKey">Category key.</param>
    /// <param name="culture">Culture (e.g. "ru", "uk", "en").</param>
    /// <returns>First word to show, or <see langword="null"/> if category/language has no words.</returns>
    public string? StartRound(long chatId, string categoryKey, string culture)
    {
        if (!_wordService.HasCategory(categoryKey))
            return null;
        var state = _wordService.StartRound(categoryKey, culture);
        _sessions[chatId] = state;
        var words = state.ShuffledWords;
        return words.Count > 0 ? words[0] : null;
    }

    /// <summary>Marks current word as guessed, increments score, and returns the next word.</summary>
    /// <param name="chatId">Chat ID.</param>
    /// <returns>Next word and new score; next word is <see langword="null"/> when round has no more words.</returns>
    public (string? NextWord, int Score) WordGuessed(long chatId)
    {
        if (!_sessions.TryGetValue(chatId, out var state))
            return (null, 0);
        state.Score++;
        var next = _wordService.GetNextWord(state);
        return (next, state.Score);
    }

    /// <summary>Skips the current word and returns the next one (no score change).</summary>
    /// <param name="chatId">Chat ID.</param>
    /// <returns>Next word, or <see langword="null"/> if no session or no words.</returns>
    public string? WordSkipped(long chatId)
    {
        if (!_sessions.TryGetValue(chatId, out var state))
            return null;
        return _wordService.GetNextWord(state);
    }

    /// <summary>Gets the current word for the chat (word at current index).</summary>
    public string? GetCurrentWord(long chatId)
    {
        if (!_sessions.TryGetValue(chatId, out var state))
            return null;
        var words = state.ShuffledWords;
        if (state.CurrentIndex >= words.Count)
            return null;
        return words[state.CurrentIndex];
    }

    /// <summary>Whether the 60-second round is still active for the chat.</summary>
    public bool IsRoundActive(long chatId) =>
        _sessions.TryGetValue(chatId, out var state) && state.IsRoundActive;

    /// <summary>Gets the current score for the chat.</summary>
    public int GetScore(long chatId) =>
        _sessions.TryGetValue(chatId, out var state) ? state.Score : 0;

    /// <summary>Ends the round and removes the session for the chat.</summary>
    public void EndRound(long chatId) => _sessions.Remove(chatId);

    /// <summary>Whether the chat has an active session (from the new word service).</summary>
    public bool HasSession(long chatId) => _sessions.ContainsKey(chatId);
}
