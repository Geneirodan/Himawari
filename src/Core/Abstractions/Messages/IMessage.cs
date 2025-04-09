using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Core.Abstractions.Messages;

public interface IMessage : IBaseRequest
{
    Message? Message { get; }
}