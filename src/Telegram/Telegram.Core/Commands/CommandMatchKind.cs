namespace Himawari.Telegram.Core.Commands;

/// <summary>
/// Kind of command resolution result: exact match, alias match, fuzzy (typo) suggestions, or not found.
/// </summary>
public enum CommandMatchKind
{
    /// <summary>Input matched a command keyword exactly (case-insensitive).</summary>
    Exact,

    /// <summary>Input matched a registered alias for a command.</summary>
    Alias,

    /// <summary>Input did not match but fuzzy (Levenshtein) suggestions are available.</summary>
    Fuzzy,

    /// <summary>No command or alias matched; no suggestions.</summary>
    NotFound
}
