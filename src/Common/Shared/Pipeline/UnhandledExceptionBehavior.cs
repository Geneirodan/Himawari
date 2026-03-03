using MediatR;
using Microsoft.Extensions.Logging;

namespace Himawari.Shared.Pipeline;

/// <summary>
/// MediatR pipeline behavior that logs any unhandled exception for the request and rethrows it.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="logger">Logger for the request type.</param>
public sealed partial class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<TRequest> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
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