using Himawari.Telegram.Core.Localization;
using Microsoft.Extensions.Localization;

namespace Himawari.Telegram.Application.Localization;

/// <summary>
/// Delegates to <see cref="IStringLocalizer{T}"/> for <see cref="BotMessages"/>.
/// Culture is resolved from <see cref="System.Globalization.CultureInfo.CurrentUICulture"/>,
/// set per-request by <see cref="Himawari.Telegram.Core.Pipeline.CultureBehavior{TRequest,TResponse}"/> and <see cref="Himawari.Telegram.Core.Pipeline.LocalizationBehavior{TRequest,TResponse}"/>.
/// </summary>
internal sealed class BotLocalizer(IStringLocalizer<BotMessages> inner) : IBotLocalizer
{
    /// <inheritdoc />
    public string this[string key] => inner[key].Value;

    /// <inheritdoc />
    public string this[string key, params object[] args] => inner[key, args].Value;
}
