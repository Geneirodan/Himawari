using Himawari.Core.Abstractions.Messages;
using Himawari.Core.Services;
using MediatR;

namespace Himawari.Core.Pipeline;

public sealed class LocalizationBehavior<TRequest, TResponse>(ILanguageResolver resolver)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request.Message is null)
            return await next(cancellationToken).ConfigureAwait(false);

        var culture = await resolver.GetCurrentCulture(request.Message.Chat.Id).ConfigureAwait(false);
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;

        return await next(cancellationToken).ConfigureAwait(false);
    }
}