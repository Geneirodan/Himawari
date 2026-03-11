namespace Himawari.Alias.Enums;

/// <summary>
/// Type of Alias game callback (inline button action).
/// </summary>
public enum AliasCallbackType
{
    /// <summary>No action.</summary>
    None = 0,
    /// <summary>Choose presenter.</summary>
    Choose,
    /// <summary>End the game.</summary>
    EndGame,
    /// <summary>Reveal current word.</summary>
    SeeWord,
    /// <summary>Skip to next word.</summary>
    NextWord,
    /// <summary>Confirm the word was guessed correctly (advance to next word, increment score).</summary>
    Correct,
    /// <summary>Skip the current word (same as <see cref="NextWord"/>).</summary>
    Skip,
    /// <summary>Stop the round/game (same as <see cref="EndGame"/>).</summary>
    Stop
}