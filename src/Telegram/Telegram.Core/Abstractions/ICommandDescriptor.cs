using Himawari.Telegram.Core.Abstractions.Messages;
using JetBrains.Annotations;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions;

[PublicAPI]
public interface ICommandDescriptor
{
    string Description { get; }
    string Keyword { get; }
    Func<Message, string, ITelegramCommand> Factory { get; }
    IReadOnlySet<string> Aliases { get; }
}
