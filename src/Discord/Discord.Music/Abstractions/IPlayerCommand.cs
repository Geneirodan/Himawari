using DisCatSharp.Lavalink;
using Himawari.Discord.Core.Abstractions;
using Mediator;

namespace Himawari.Discord.Music.Abstractions;

public interface IPlayerCommand : IDiscordCommand
{
    LavalinkGuildPlayer Player { get; set; }
    bool ShouldAutoConnect { get; }
}
