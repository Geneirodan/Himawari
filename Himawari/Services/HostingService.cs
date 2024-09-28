using Himawari.Abstractions.Services;
using WTelegram;

namespace Himawari.Services;

internal class HostingService(
    HttpClient client,
    IConfiguration configuration,
    IDispatcher dispatcher,
    Bot bot,
    ILogger<HostingService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        bot.OnMessage += dispatcher.OnMessage;
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