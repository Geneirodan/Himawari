using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions.Messages;

public interface IMessage : IBaseRequest
{
    Message? Message { get; }
}