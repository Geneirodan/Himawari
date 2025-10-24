using MediatR;
using Microsoft.Extensions.Logging;

namespace Himawari.Telegram.Core.Pipeline;

public sealed partial class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogException(ex, typeof(TRequest).Name);
            throw;
        }
    }

    [LoggerMessage(LogLevel.Error, "Request: Unhandled Exception for command {CommandName}")]
    private partial void LogException(Exception exception, string commandName);
}