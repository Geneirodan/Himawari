using MediatR;

namespace Himawari.Web.Pipeline;

public sealed class UnhandledExceptionBehavior<TRequest, TResponse>(ILogger<TRequest> logger) 
    : IPipelineBehavior<TRequest, TResponse> 
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Request: Unhandled Exception for command {Name} {@Request}", 
                typeof(TRequest).Name, request);

            throw;
        }
    }
}