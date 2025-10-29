using DisCatSharp.Lavalink.Entities;

namespace Himawari.Discord.Music.Extensions;

public static class TrackExtensions
{
    public static string CreateTrackName(this LavalinkTrack track) => track.Info.Author + " - " + track.Info.Title;
}