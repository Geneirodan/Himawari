using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Abstractions;

public interface ICommand : IRequest<Message>
{
    Message Message { get; }
}