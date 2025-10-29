using DisCatSharp;
using DisCatSharp.Lavalink;
using Himawari.Discord.Core;
using Himawari.Discord.Music;
using Microsoft.Extensions.Options;

namespace Himawari.Service.Services;

public sealed class DiscordBackgroundService(DiscordClient client, IOptions<DiscordOptions> options, LavalinkConfiguration lavalinkConfiguration) : BackgroundService
{
    private readonly DiscordOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await client.ConnectAsync(_options.Activity).ConfigureAwait(false);
        await client.GetLavalink().ConnectAsync(lavalinkConfiguration).ConfigureAwait(false);
        await Task.Delay(-1, cancellationToken).ConfigureAwait(false);
    }
}