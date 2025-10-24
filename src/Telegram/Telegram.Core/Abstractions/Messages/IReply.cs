using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions.Messages;

public interface IReply : IMessage, IRequest<IEnumerable<Message>>;