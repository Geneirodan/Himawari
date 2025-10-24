using DisCatSharp.Net;

namespace Himawari.Discord.Music;

public sealed record DiscordOptions
{
    public required string Token { get; init; }
    public required ConnectionEndpoint Lavalink { get; init; }
}