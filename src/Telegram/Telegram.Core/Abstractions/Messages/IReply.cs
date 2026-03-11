using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions.Messages;

/// <summary>
/// A Telegram message request that returns multiple <see cref="Message"/> instances as the reply.
/// </summary>
public interface IReply : IMessage, IRequest<IEnumerable<Message>>;