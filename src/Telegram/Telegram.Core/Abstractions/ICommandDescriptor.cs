using Himawari.Telegram.Core.Abstractions.Messages;
using JetBrains.Annotations;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions;

/// <summary>
/// Describes a Telegram bot command: keyword, description, aliases, and a factory to create an <see cref="ICommand"/> from a message and parsed text.
/// Used by <see cref="ICommandResolver"/> to build the command list and resolve commands from messages.
/// </summary>
[PublicAPI]
public interface ICommandDescriptor
{
    /// <summary>Short description shown in the bot command list.</summary>
    string Description { get; }
    /// <summary>Primary command keyword (e.g. /start).</summary>
    string Keyword { get; }
    /// <summary>Creates an <see cref="ICommand"/> instance from the incoming <paramref name="Message"/> and the parsed command argument string.</summary>
    Func<Message, string, ICommand> Factory { get; }
    /// <summary>Alternative keywords that trigger the same command.</summary>
    IReadOnlySet<string> Aliases { get; }
}