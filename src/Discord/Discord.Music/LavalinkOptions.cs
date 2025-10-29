using DisCatSharp.Net;

namespace Himawari.Discord.Music;

public sealed record LavalinkOptions
{
    public ConnectionEndpoint Endpoint { get; init; }
}