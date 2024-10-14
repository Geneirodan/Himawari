using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Core.Abstractions;

public interface ICommand : IRequest<Message>
{
    Message Message { get; }
}