using WTelegram;

namespace Himawari.Services;

internal class HostingService(ICommandService commandService, IDispatcher dispatcher, Bot bot) : BackgroundService
{
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        bot.OnMessage += dispatcher.OnMessage;
        commandService.Configure(bot);
        return Task.Delay(-1, stoppingToken);
    }
}