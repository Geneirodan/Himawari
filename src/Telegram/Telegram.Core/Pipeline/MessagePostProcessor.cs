using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Pipeline;

/// <summary>
/// MediatR post-processor for commands that return a <see cref="Message"/>; logs the sent message ID.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type (Message).</typeparam>
/// <param name="logger">Logger for the request type.</param>
public sealed partial class MessagePostProcessor<TRequest, TResponse>(ILogger<TRequest> logger)
    : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : IRequest<Message>
    where TResponse : Message?
{
    /// <inheritdoc />
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (response is not null)
            LogMessage(response.MessageId);
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "Message sent with id: {Id}")]
    private partial void LogMessage(int id);
}