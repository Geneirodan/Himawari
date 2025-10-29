using DisCatSharp.Lavalink;
using Himawari.Discord.Core.Abstractions;

namespace Himawari.Discord.Music.Abstractions;

public interface IPlayerCommand : IDiscordCommand
{
    LavalinkGuildPlayer Player { get; set; }
    bool ShouldAutoConnect { get; }
}