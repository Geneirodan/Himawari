namespace Himawari.Telegram.Core.Localization;

/// <summary>
/// Strongly-typed resource keys for <see cref="IBotLocalizer"/>.
/// Prevents magic-string bugs; IDE shows usages and renames propagate.
/// </summary>
public static class Loc
{
    /// <summary>Start command: greeting in private chat.</summary>
    public const string StartGreeting = nameof(StartGreeting);

    /// <summary>Start command: prompt to pick a command.</summary>
    public const string StartPickCommand = nameof(StartPickCommand);

    /// <summary>Who command: prompt to pick presenter.</summary>
    public const string WhoPickPrompt = nameof(WhoPickPrompt);

    /// <summary>Who command: result line (format: {0} = presenter name).</summary>
    public const string WhoPickResult = nameof(WhoPickResult);

    /// <summary>Who command: no members in chat.</summary>
    public const string WhoNoMembers = nameof(WhoNoMembers);

    /// <summary>Who command: cannot list chat members.</summary>
    public const string WhoCantListMembers = nameof(WhoCantListMembers);

    /// <summary>Gift command: prompt who gets the gift.</summary>
    public const string GiftPrompt = nameof(GiftPrompt);

    /// <summary>Gift command: result line (format: {0} = username).</summary>
    public const string GiftResult = nameof(GiftResult);

    /// <summary>Alias game: start message.</summary>
    public const string AliasStart = nameof(AliasStart);

    /// <summary>Alias game: time's up.</summary>
    public const string AliasTimeUp = nameof(AliasTimeUp);

    /// <summary>Alias game: correct guess.</summary>
    public const string AliasCorrect = nameof(AliasCorrect);

    /// <summary>Alias game: skip.</summary>
    public const string AliasSkip = nameof(AliasSkip);

    /// <summary>Alias game: scoreboard (format: {0} = lines).</summary>
    public const string AliasScoreboard = nameof(AliasScoreboard);

    /// <summary>Alias game: game already running.</summary>
    public const string AliasAlreadyRunning = nameof(AliasAlreadyRunning);

    /// <summary>Alias game: choose category.</summary>
    public const string AliasChooseCategory = nameof(AliasChooseCategory);

    /// <summary>Todo: item added (format: {0} = text).</summary>
    public const string TodoAdded = nameof(TodoAdded);

    /// <summary>Todo: list header (format: {0} = items).</summary>
    public const string TodoList = nameof(TodoList);

    /// <summary>Todo: empty list.</summary>
    public const string TodoEmpty = nameof(TodoEmpty);

    /// <summary>Help command: header.</summary>
    public const string HelpHeader = nameof(HelpHeader);

    /// <summary>Help command: footer.</summary>
    public const string HelpFooter = nameof(HelpFooter);

    /// <summary>Generic error message.</summary>
    public const string ErrorGeneric = nameof(ErrorGeneric);

    /// <summary>No permission error.</summary>
    public const string ErrorNoPermission = nameof(ErrorNoPermission);

    /// <summary>/lang: unsupported language (format: {0} = list of supported codes).</summary>
    public const string LanguageNotFound = nameof(LanguageNotFound);

    /// <summary>/lang: confirmation (format: {0} = language code).</summary>
    public const string LanguageSet = nameof(LanguageSet);

    /// <summary>Fuzzy command suggestion (format: {0} = list of /command links).</summary>
    public const string UnknownCommandDidYouMean = nameof(UnknownCommandDidYouMean);
}
