using DisCatSharp.Entities;

namespace Himawari.Discord.Core;

public sealed record DiscordOptions
{
    public required string Token { get; init; }
    public required DiscordActivity Activity { get; init; }
}