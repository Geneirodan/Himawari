using DisCatSharp.Entities;

namespace Himawari.Discord.Core;

/// <summary>
/// Configuration for the Discord bot: token and optional activity (e.g. "Listening to ...").
/// </summary>
public sealed record DiscordOptions
{
    /// <summary>Discord bot token.</summary>
    public required string Token { get; init; }
    /// <summary>Activity shown in the bot's presence.</summary>
    public required DiscordActivity Activity { get; init; }
}