using Himawari.Core.Abstractions.Messages;
using JetBrains.Annotations;
using Telegram.Bot.Types;

namespace Himawari.Core.Abstractions;

[PublicAPI]
public interface ICommandDescriptor
{
    string Description { get; }
    string Keyword { get; }
    public Func<Message, string, ICommand> Factory { get; }
    public IReadOnlySet<string> Aliases { get; }
}