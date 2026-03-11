using System.Collections.Generic;
using System.Globalization;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Commands;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Services;

/// <summary>
/// Resolves Telegram bot commands by name or alias and provides the command list per culture for <see cref="Bot.SetMyCommands"/>.
/// </summary>
public interface ICommandResolver
{
    /// <summary>Returns the command factory for the given command name, or <see langword="null"/> if not found.</summary>
    Func<Message, string, ICommand>? GetFactoryByName(string commandName);
    /// <summary>Returns the primary command keyword for the given alias, or <see langword="null"/> if not found.</summary>
    string? GetCommandByAlias(string alias);
    /// <summary>Returns the list of bot commands for the given culture (for setting MyCommands per locale).</summary>
    IEnumerable<BotCommand> GetCommandsByCulture(CultureInfo cultureInfo);
    /// <summary>Resolves input (command token without leading slash) using multi-tier matching: exact, alias, fuzzy. See <see cref="CommandMatchResult"/>.</summary>
    CommandMatchResult Resolve(ReadOnlySpan<char> input, int? maxDistance = null);
    /// <summary>Returns a case-insensitive lookup from natural-language phrase to canonical command name (without slash). Used for fast NL trigger resolution.</summary>
    IReadOnlyDictionary<string, string> GetAlternateLookup();
}