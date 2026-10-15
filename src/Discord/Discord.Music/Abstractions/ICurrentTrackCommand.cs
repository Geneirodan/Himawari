using DisCatSharp.Lavalink.Entities;
using Mediator;

namespace Himawari.Discord.Music.Abstractions;

public interface ICurrentTrackCommand : IPlayerCommand
{
    LavalinkTrack CurrentTrack { get; set; }
}
