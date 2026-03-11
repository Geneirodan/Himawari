using Himawari.Alias.Enums;

namespace Himawari.Alias.Services;

/// <summary>
/// Service for the Alias game: start/end game, get next word, verify guesses, and resolve presenter per chat.
/// </summary>
public interface IAliasService
{
    /// <summary>Sets the pending word-set category for the chat (e.g. before choosing presenter). Used when the user picks a category from the inline keyboard.</summary>
    void SetCategory(long chatId, string categoryKey);
    /// <summary>Gets the pending category for the chat, or <see langword="null"/> if none. Cleared after <see cref="StartAsync"/> uses it.</summary>
    string? GetCategory(long chatId);
    /// <summary>Starts a new game in the chat with the given presenter and optional category. Uses pending category from <see cref="SetCategory"/> if <paramref name="categoryKey"/> is null.</summary>
    /// <param name="chatId">Telegram chat ID.</param>
    /// <param name="presenterId">User ID of the presenter.</param>
    /// <param name="categoryKey">Category key (e.g. "animals", "random"), or null to use the pending one.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result is the initial word or status text.</returns>
    Task<string> StartAsync(long chatId, long presenterId, string? categoryKey = null, CancellationToken cancellationToken = default);
    /// <summary>Ends the current game in the chat.</summary>
    /// <param name="chatId">Telegram chat ID.</param>
    void EndGame(long chatId);
    /// <summary>Gets the current presenter user ID for the chat, or <see langword="null"/> if no game.</summary>
    long? GetPresenterId(long chatId);
    /// <summary>Advances to the next word and returns it, or <see langword="null"/> if none.</summary>
    Task<string?> NextWordAsync(long chatId, CancellationToken cancellationToken = default);
    /// <summary>Gets or creates the current word for the chat.</summary>
    Task<string?> GetOrCreateCurrentWordAsync(long chatId, CancellationToken cancellationToken = default);
    /// <summary>Verifies a guessed word against the current word and returns the result (<see cref="Guess.Correct"/>, Partial, or Incorrect).</summary>
    Guess VerifyWord(long chatId, string word);
    /// <summary>Gets the number of words guessed correctly in the current game (per chat).</summary>
    int GetCorrectCount(long chatId);
    /// <summary>Increments the correct-guess count for the chat and returns the new count.</summary>
    int IncrementCorrectCount(long chatId);
}