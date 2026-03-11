namespace Himawari.Alias.Enums;

/// <summary>
/// Result of verifying a word guess in the Alias game.
/// </summary>
public enum Guess
{
    /// <summary>Guess does not match.</summary>
    Incorrect,
    /// <summary>Guess partially matches (e.g. typo or substring).</summary>
    Partial,
    /// <summary>Guess is correct.</summary>
    Correct
}