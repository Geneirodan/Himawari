using Himawari.Core.Abstractions;
using Himawari.Core.Services;
using MediatR;

namespace Himawari.Core.Pipeline;

public sealed class LocalizationBehavior<TRequest, TResponse>(ILanguageResolver resolver)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IResponse
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var culture = await resolver.GetCurrentCulture(request.Message.Chat.Id).ConfigureAwait(false);
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        return await next(cancellationToken).ConfigureAwait(false);
    }
}