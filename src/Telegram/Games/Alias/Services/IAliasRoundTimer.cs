namespace Himawari.Alias.Services;

/// <summary>
/// Per-chat round timer for the Alias game. When the duration elapses, the callback is invoked (e.g. send "Time's up" and end the game).
/// Cancel when the user ends the game manually so the callback does not run.
/// </summary>
public interface IAliasRoundTimer
{
    /// <summary>Starts a round timer for the chat. When <paramref name="duration"/> elapses, <paramref name="onElapsed"/> is invoked. Replaces any existing timer for the chat.</summary>
    /// <param name="chatId">Telegram chat ID.</param>
    /// <param name="duration">Round duration.</param>
    /// <param name="onElapsed">Callback to run when the timer elapses (e.g. send message and end game). Receives the chat ID and a cancellation token.</param>
    void Start(long chatId, TimeSpan duration, Func<long, CancellationToken, Task> onElapsed);

    /// <summary>Cancels the round timer for the chat if one is running. No-op if no timer.</summary>
    /// <param name="chatId">Telegram chat ID.</param>
    void Cancel(long chatId);
}
