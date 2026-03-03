using System.Globalization;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Localization;
using MediatR;

namespace Himawari.Telegram.Core.Pipeline;

/// <summary>
/// MediatR pipeline behavior that overrides the culture set by <see cref="CultureBehavior{TRequest,TResponse}"/> with the explicit language stored via /lang.
/// Registration order (outermost → innermost): CultureBehavior → LocalizationBehavior → Handler.
/// When the chat has no /lang preference, leaves the culture from CultureBehavior (LanguageCode). When it has one, overrides.
/// </summary>
/// <typeparam name="TRequest">The request type (must be <see cref="IMessage"/>).</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="resolver">Resolves explicit chat language from /lang; returns null when not set.</param>
public sealed class LocalizationBehavior<TRequest, TResponse>(IExplicitLanguageResolver resolver)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IMessage
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request.Message is not null)
        {
            var culture = await resolver.ResolveAsync(request.Message.Chat.Id, cancellationToken).ConfigureAwait(false);
            if (culture is not null)
            {
                Thread.CurrentThread.CurrentUICulture = culture;
                Thread.CurrentThread.CurrentCulture = culture;
            }
        }

        return await next().ConfigureAwait(false);
    }
}
