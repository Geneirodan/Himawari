using System.Globalization;
using Himawari.Telegram.Core.Options;
using Himawari.Telegram.Core.Services;
using Microsoft.Extensions.Options;
using WTelegram;

namespace Himawari.Service.Services;

/// <summary>
/// Hosted service that drops pending Telegram updates on startup and periodically refreshes the bot command list for each configured locale using <see cref="ICommandResolver"/>.
/// </summary>
internal sealed class TelegramBackgroundService(
    Bot bot,
    ICommandResolver resolver,
    IOptionsMonitor<BotOptions> optionsMonitor)
    : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await bot.DropPendingUpdates();
        await UpdateCommands(cancellationToken);
    }

    /// <summary>
    /// Registers command list for each <see cref="BotOptions.SupportedLocales"/> (en, uk, ru).
    /// Telegram shows the menu in the user's app language; descriptions are loaded from resources per culture.
    /// </summary>
    private async Task UpdateCommands(CancellationToken cancellationToken)
    {
        do
        {
            var first = true;
            foreach (var locale in optionsMonitor.CurrentValue.SupportedLocales)
            {
                if (first)
                {
                    await SetCommandsForLocale(locale);
                    first = false;
                }

                await SetCommandsForLocale(locale, locale);
            }

            var timeout = optionsMonitor.CurrentValue.PingTimeout;
            if (timeout.TotalSeconds > 0)
                await Task.Delay(timeout, cancellationToken);
            else
                await Task.Delay(-1, cancellationToken);
        } while (!cancellationToken.IsCancellationRequested);
    }

    private async Task SetCommandsForLocale(string locale, string? languageCode = null)
    {
        var cultureInfo = CultureInfo.GetCultureInfo(locale);
        var commands = resolver.GetCommandsByCulture(cultureInfo);
        await bot.SetMyCommands(commands, languageCode: languageCode);
    }
}