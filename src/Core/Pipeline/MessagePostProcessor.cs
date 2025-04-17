using MediatR;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Himawari.Core.Pipeline;

public sealed partial class MessagePostProcessor<TRequest, TResponse>(ILogger<TRequest> logger)
    : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : IRequest<Message>
    where TResponse : Message?
{
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (response is not null)
            LogMessage(response.MessageId);
        return Task.CompletedTask;
    }

    [LoggerMessage(LogLevel.Information, "Message sent with id: {Id}")]
    private partial void LogMessage(int id);
}