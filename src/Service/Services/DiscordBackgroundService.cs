using DisCatSharp;
using DisCatSharp.Lavalink;
using Himawari.Discord.Core;
using Microsoft.Extensions.Options;

namespace Himawari.Service.Services;

/// <summary>
/// Hosted service that connects the Discord client and Lavalink, then keeps running. Start after the host is built.
/// </summary>
public sealed class DiscordBackgroundService(DiscordClient client, IOptions<DiscordOptions> options, LavalinkConfiguration lavalinkConfiguration) : BackgroundService
{
    private readonly DiscordOptions _options = options.Value;

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await client.ConnectAsync(_options.Activity).ConfigureAwait(false);
        await client.GetLavalink().ConnectAsync(lavalinkConfiguration).ConfigureAwait(false);
        await Task.Delay(-1, cancellationToken).ConfigureAwait(false);
    }
}