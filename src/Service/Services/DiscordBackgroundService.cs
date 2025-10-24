using DisCatSharp;
using DisCatSharp.Lavalink;
using Himawari.Discord.Music;
using Microsoft.Extensions.Options;

namespace Himawari.Service.Services;

public sealed class DiscordBackgroundService(DiscordClient client, IOptions<DiscordOptions> options) : BackgroundService
{
    private readonly DiscordOptions _options = options.Value;
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await client.ConnectAsync().ConfigureAwait(false);
        var lavalinkConfig = new LavalinkConfiguration
        {
            RestEndpoint = _options.Lavalink,
            SocketEndpoint = _options.Lavalink,
            EnableBuiltInQueueSystem = true
        };
        await client.GetLavalink().ConnectAsync(lavalinkConfig).ConfigureAwait(false);
        await Task.Delay(-1, cancellationToken).ConfigureAwait(false);
    }
}