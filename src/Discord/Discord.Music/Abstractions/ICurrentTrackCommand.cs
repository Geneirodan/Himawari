using DisCatSharp.Lavalink.Entities;

namespace Himawari.Discord.Music.Abstractions;

/// <summary>
/// Discord music command that operates on the current track (e.g. now playing, skip).
/// </summary>
public interface ICurrentTrackCommand : IPlayerCommand
{
    /// <summary>The currently playing Lavalink track, if any.</summary>
    LavalinkTrack CurrentTrack { get; set; }
}