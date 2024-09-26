using Himawari.Services;
using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Commands;

public interface ICommand: IRequest<Message>
{
    Message Message { get; init; }
    CommandInfo CommandInfo { get; init; }
}