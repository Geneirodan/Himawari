using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Core.Abstractions;

public interface IResponse : IRequest<Message>
{
    public Message Message { get; }
}