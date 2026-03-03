using System.Globalization;
using Himawari.Telegram.Core.Abstractions.Messages;
using MediatR;

namespace Himawari.Telegram.Core.Localization;

/// <summary>
/// MediatR pipeline behavior: sets <see cref="CultureInfo.CurrentUICulture"/> and <see cref="CultureInfo.CurrentCulture"/> per request.
/// Delegates resolution to <see cref="ICultureResolver"/> (e.g. <see cref="LanguageFallbackPolicy"/>).
/// Runs before <see cref="Pipeline.LocalizationBehavior{TRequest,TResponse}"/>, which may override with DB-stored culture.
/// </summary>
/// <typeparam name="TRequest">The request type.</typeparam>
/// <typeparam name="TResponse">The response type.</typeparam>
/// <param name="resolver">Resolves culture from Telegram LanguageCode.</param>
public sealed class CultureBehavior<TRequest, TResponse>(ICultureResolver resolver) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    /// <inheritdoc />
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var lang = request is IMessage { Message: not null } msg ? msg.Message.From?.LanguageCode : null;
        var culture = resolver.Resolve(lang);
        Thread.CurrentThread.CurrentUICulture = culture;
        Thread.CurrentThread.CurrentCulture = culture;
        return await next().ConfigureAwait(false);
    }
}
