namespace Himawari.Telegram.Core.Commands;

/// <summary>
/// Result of resolving user input to a command using multi-tier matching (exact, alias, fuzzy, or not found).
/// </summary>
public sealed record CommandMatchResult
{
    /// <summary>Kind of match.</summary>
    public CommandMatchKind Kind { get; private init; }

    /// <summary>The matched command entry when <see cref="Kind"/> is <see cref="CommandMatchKind.Exact"/> or <see cref="CommandMatchKind.Alias"/>.</summary>
    public CommandEntry? Entry { get; private init; }

    /// <summary>The alias string that matched when <see cref="Kind"/> is <see cref="CommandMatchKind.Alias"/>.</summary>
    public string? MatchedAlias { get; private init; }

    /// <summary>Suggested command names (without slash) when <see cref="Kind"/> is <see cref="CommandMatchKind.Fuzzy"/>.</summary>
    public string[]? Suggestions { get; private init; }

    /// <summary>Original input when <see cref="Kind"/> is <see cref="CommandMatchKind.NotFound"/>.</summary>
    public string? OriginalInput { get; private init; }

    /// <summary>Creates a result for an exact keyword match.</summary>
    public static CommandMatchResult Exact(CommandEntry entry) =>
        new() { Kind = CommandMatchKind.Exact, Entry = entry };

    /// <summary>Creates a result for an alias match.</summary>
    public static CommandMatchResult Alias(CommandEntry entry, string alias) =>
        new() { Kind = CommandMatchKind.Alias, Entry = entry, MatchedAlias = alias };

    /// <summary>Creates a result with typo suggestions (e.g. "Did you mean /help?").</summary>
    public static CommandMatchResult Fuzzy(string[] suggestions) =>
        new() { Kind = CommandMatchKind.Fuzzy, Suggestions = suggestions };

    /// <summary>Creates a result when no command or alias matched.</summary>
    public static CommandMatchResult NotFound(string input) =>
        new() { Kind = CommandMatchKind.NotFound, OriginalInput = input };
}
