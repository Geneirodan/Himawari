using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions.Messages;

/// <summary>
/// A Telegram bot command that returns a single <see cref="Message"/> as the reply.
/// </summary>
public interface ICommand : IMessage, IRequest<Message>;