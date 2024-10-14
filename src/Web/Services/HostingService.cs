using Himawari.SpellChecking;
using Himawari.Web.Services.Abstractions;
using WTelegram;

namespace Himawari.Web.Services;

internal class HostingService(
    HttpClient client,
    IConfiguration configuration,
    IDispatcher dispatcher,
    IKeyboardLayoutService layoutService,
    ILanguageSync languageSync,
    Bot bot,
    ILogger<HostingService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        bot.OnMessage += languageSync.OnMessage;
        bot.OnMessage += dispatcher.OnMessage;
        bot.OnMessage += layoutService.OnMessage;
        
        bot.OnUpdate += languageSync.OnUpdate;
        bot.OnUpdate += dispatcher.OnUpdate;
        
        await bot.DropPendingUpdates();
        if (configuration.GetValue<string>("BaseAddress") is { } baseAddress)
        {
            client.BaseAddress = new Uri(baseAddress);
            await Ping(cancellationToken);
        }
        else
            await Task.Delay(-1, cancellationToken);
    }

    private async Task Ping(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
            if (client is not null)
            {
                var response = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/"), cancellationToken);
                logger.LogInformation("Ping: {Code}", response.StatusCode);
                await Task.Delay(10000, cancellationToken);
            }
    }
}