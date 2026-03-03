using System.Globalization;
using Himawari.Telegram.Core.Localization;
using Microsoft.Extensions.Caching.Memory;

namespace Himawari.Telegram.Core.Services;

/// <summary>
/// Resolves explicit chat language from <see cref="ILanguageRepository"/> and maps to <see cref="CultureInfo"/> via <see cref="ICultureResolver"/>.
/// Returns <see langword="null"/> when the chat has no stored /lang preference.
/// </summary>
public sealed class ExplicitLanguageResolver(ILanguageRepository repo, ICultureResolver cultureResolver) : IExplicitLanguageResolver
{
    /// <inheritdoc />
    public async Task<CultureInfo?> ResolveAsync(long chatId, CancellationToken cancellationToken = default)
    {
        var code = await repo.GetAsync(chatId, cancellationToken).ConfigureAwait(false);
        return code is null ? null : cultureResolver.Resolve(code);
    }
}
