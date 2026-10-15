using Mediator;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions.Messages;

public interface IReply : IRequest<IEnumerable<Message>>;
