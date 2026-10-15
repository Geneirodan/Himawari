using Mediator;
using Microsoft.Extensions.Logging;
using Telegram.Bot.Types;

namespace Himawari.Telegram.Core.Pipeline;

public sealed partial class TelegramMessagePostProcessor<TRequest, TResponse>(ILogger<TRequest> logger)
    : MessagePostProcessor<TRequest, TResponse>
    where TRequest : IRequest<Message>
    where TResponse : Message?
{
    [LoggerMessage(LogLevel.Information, "Message sent with id: {Id}")]
    private partial void LogMessage(int id);

    protected override ValueTask Handle(TRequest message, TResponse response, CancellationToken cancellationToken)
    {
        if (response is not null)
            LogMessage(response.MessageId);
        return ValueTask.CompletedTask;
    }
}
