using Himawari.Telegram.Core.Abstractions.Messages;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Commands;

/// <summary>
/// Immutable entry for a registered command: keyword (with leading slash), description for the bot command list, and factory to create the command instance.
/// </summary>
/// <param name="Keyword">Primary command keyword including leading slash (e.g. /help).</param>
/// <param name="Description">Short description for <see cref="Bot.SetMyCommands"/> and help.</param>
/// <param name="Factory">Creates an <see cref="ICommand"/> from message and argument string.</param>
public sealed record CommandEntry(
    string Keyword,
    string Description,
    Func<Message, string, ICommand> Factory);
