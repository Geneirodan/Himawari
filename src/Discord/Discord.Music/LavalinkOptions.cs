using DisCatSharp.Net;

namespace Himawari.Discord.Music;

/// <summary>
/// Configuration for the Lavalink connection (REST and socket endpoint).
/// </summary>
public sealed record LavalinkOptions
{
    /// <summary>Lavalink server endpoint (host and port).</summary>
    public ConnectionEndpoint Endpoint { get; init; }
}