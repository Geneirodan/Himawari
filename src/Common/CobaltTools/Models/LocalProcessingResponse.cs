using System.Text.Json.Serialization;
using JetBrains.Annotations;
using static Himawari.CobaltTools.Models.LocalProcessingResponse;

namespace Himawari.CobaltTools.Models;

/// <summary>
/// CobaltTools API response when media is processed locally (merge, mute, audio, gif, remux). Contains tunnel URLs, output and audio metadata.
/// </summary>
/// <param name="Type">Processing mode (e.g. <see cref="LocalProcessingType.Merge"/>, <see cref="LocalProcessingType.Audio"/>).</param>
/// <param name="Service">Service identifier.</param>
/// <param name="Tunnel">URLs to fetch the result (e.g. stream endpoints).</param>
/// <param name="Output">Output file metadata and optional subtitles.</param>
/// <param name="Audio">Audio stream details (copy, format, bitrate, cover).</param>
/// <param name="IsHls">Whether the output is HLS.</param>
[PublicAPI]
public sealed record LocalProcessingResponse(
    LocalProcessingType Type,
    string Service,
    string[] Tunnel,
    OutputObject Output,
    AudioObject Audio,
    bool? IsHls
) : CobaltToolsResponse
{
    /// <summary>
    /// Output file description: type, filename, metadata, and subtitles flag.
    /// </summary>
    /// <param name="Type">Output type.</param>
    /// <param name="Filename">Output filename.</param>
    /// <param name="Metadata">Optional metadata (album, artist, title, etc.).</param>
    /// <param name="Subtitles">Whether subtitles are included.</param>
    public sealed record OutputObject(
        string Type,
        string Filename,
        OutputObject.MetadataObject Metadata,
        bool Subtitles)
    {
        /// <summary>
        /// Optional media metadata (album, artist, title, track, date, etc.).
        /// </summary>
        [PublicAPI]
        public sealed record MetadataObject
        {
            /// <summary>Album name.</summary>
            public string? Album { get; init; }
            /// <summary>Composer.</summary>
            public string? Composer { get; init; }
            /// <summary>Genre.</summary>
            public string? Genre { get; init; }
            /// <summary>Copyright.</summary>
            public string? Copyright { get; init; }
            /// <summary>Title.</summary>
            public string? Title { get; init; }
            /// <summary>Artist.</summary>
            public string? Artist { get; init; }
            /// <summary>Album artist.</summary>
            [JsonPropertyName("album_artist")]
            public string? AlbumArtist { get; init; }
            /// <summary>Track number.</summary>
            public string? Track { get; init; }
            /// <summary>Date.</summary>
            public string? Date { get; init; }
            /// <summary>Subtitle language.</summary>
            public string? Sublanguage { get; init; }
        }
    }

    /// <summary>
    /// Audio stream options: copy flag, format, bitrate, cover and crop-cover flags.
    /// </summary>
    /// <param name="Copy">Whether audio is copied without re-encoding.</param>
    /// <param name="Format">Audio format.</param>
    /// <param name="Bitrate">Bitrate.</param>
    /// <param name="Cover">Whether cover art is included.</param>
    /// <param name="CropCover">Whether cover is cropped.</param>
    public sealed record AudioObject(bool Copy, string Format, string Bitrate, bool Cover, bool CropCover);
}