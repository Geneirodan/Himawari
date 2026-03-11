using MediatR;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Abstractions.Messages;

/// <summary>
/// Base MediatR request for Telegram that carries the optional originating <see cref="Message"/>.
/// </summary>
public interface IMessage : IBaseRequest
{
    /// <summary>The Telegram message that triggered the request, if any.</summary>
    Message? Message { get; }
}