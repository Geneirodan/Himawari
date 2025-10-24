using System.Globalization;
using Himawari.Telegram.Core.Options;
using Himawari.Telegram.Core.Services;
using Microsoft.Extensions.Options;
using WTelegram;

namespace Himawari.Service.Services;

internal sealed class TelegramBackgroundService(Bot bot, ICommandResolver resolver, IOptionsMonitor<BotOptions> optionsMonitor)
    : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        bot.DropPendingUpdates();
        return UpdateCommands(cancellationToken);
    }

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
        var cultureInfo = new CultureInfo(locale);
        var commands = resolver.GetCommandsByCulture(cultureInfo);
        await bot.SetMyCommands(commands, languageCode: languageCode);
    }
}