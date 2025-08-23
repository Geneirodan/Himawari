using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Core.Abstractions.Messages;

public interface IReply : IMessage, IRequest<IEnumerable<Message>>;