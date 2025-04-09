using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Core.Abstractions.Messages;

public interface ICommand : IMessage, IRequest<Message>;