using System.Globalization;
using Himawari.Core.Options;
using Himawari.Core.Services;
using Microsoft.Extensions.Options;
using WTelegram;

namespace Himawari.Service;

internal sealed class HostingService(Bot bot, ICommandResolver resolver, IOptionsMonitor<BotOptions> optionsMonitor)
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
            foreach (var locale in optionsMonitor.CurrentValue.SupportedLocales)
            {
                var cultureInfo = new CultureInfo(locale);
                var commands = resolver.GetCommandsByCulture(cultureInfo);
                await bot.SetMyCommands(commands, languageCode: locale);
            }

            var timeout = optionsMonitor.CurrentValue.PingTimeout;
            if (timeout.TotalSeconds > 0)
                await Task.Delay(timeout, cancellationToken);
            else
                await Task.Delay(-1, cancellationToken);
        } while (!cancellationToken.IsCancellationRequested);
    }
}