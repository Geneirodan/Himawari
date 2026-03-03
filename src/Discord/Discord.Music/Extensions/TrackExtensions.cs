using DisCatSharp.Lavalink.Entities;

namespace Himawari.Discord.Music.Extensions;

/// <summary>Extension methods for <see cref="LavalinkTrack"/> (e.g. display name for messages).</summary>
public static class TrackExtensions
{
    /// <summary>Returns a display string for the track (author - title).</summary>
    public static string CreateTrackName(this LavalinkTrack track) => track.Info.Author + " - " + track.Info.Title;
}