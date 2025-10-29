using DisCatSharp.Lavalink.Entities;

namespace Himawari.Discord.Music.Abstractions;

public interface ICurrentTrackCommand : IPlayerCommand
{
    LavalinkTrack CurrentTrack { get; set; }
}