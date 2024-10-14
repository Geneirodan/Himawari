using MediatR;
using MediatR.Pipeline;
using Telegram.Bot.Types;

namespace Himawari.Web.Pipeline;

public sealed class MessagePostProcessor<TRequest, TResponse>(ILogger<TRequest> logger)
    : IRequestPostProcessor<TRequest, TResponse>
    where TRequest : IRequest<Message>
    where TResponse : Message?
{
    public Task Process(TRequest request, TResponse response, CancellationToken cancellationToken)
    {
        if (response is not null)
            logger.LogInformation("Message sent with id: {Id}", response.MessageId);
        return Task.CompletedTask;
    }
}