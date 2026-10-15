using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Services;
using Mediator;

namespace Himawari.Telegram.Core.Pipeline;

public sealed class LocalizationBehavior<TRequest, TResponse>(ILanguageResolver resolver)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ITelegramMessage
{
    public async ValueTask<TResponse> Handle(
        TRequest message,
        MessageHandlerDelegate<TRequest, TResponse> next,
        CancellationToken cancellationToken
    )
    {
        if (message.Message is null)
            return await next(message, cancellationToken).ConfigureAwait(false);

        var culture = await resolver.GetCurrentCulture(message.Message.Chat.Id).ConfigureAwait(false);
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;

        return await next(message, cancellationToken).ConfigureAwait(false);
    }
}
