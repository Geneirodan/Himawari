using Mediator;
using Microsoft.Extensions.Logging;

namespace Himawari.Shared.Pipeline;

public sealed partial class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    [LoggerMessage(LogLevel.Error, "Request: Unhandled Exception for command {CommandName}")]
    private partial void LogException(Exception exception, string commandName);

    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        try
        {
            return await next(message, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogException(ex, typeof(TRequest).Name);
            throw;
        }
    }
}
