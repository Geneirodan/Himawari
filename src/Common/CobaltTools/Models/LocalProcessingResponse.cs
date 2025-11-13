using System.Text.Json.Serialization;
using JetBrains.Annotations;
using static Himawari.CobaltTools.Models.LocalProcessingResponse;

namespace Himawari.CobaltTools.Models;

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
    public sealed record OutputObject(
        string Type,
        string Filename,
        OutputObject.MetadataObject Metadata,
        bool Subtitles)
    {
        [PublicAPI]
        public sealed record MetadataObject
        {
            public string? Album { get; init; }
            public string? Composer { get; init; }
            public string? Genre { get; init; }
            public string? Copyright { get; init; }
            public string? Title { get; init; }
            public string? Artist { get; init; }
            [JsonPropertyName("album_artist")] public string? AlbumArtist { get; init; }
            public string? Track { get; init; }
            public string? Date { get; init; }
            public string? Sublanguage { get; init; }
        }
    }
    public sealed record AudioObject(bool Copy, string Format, string Bitrate, bool Cover, bool CropCover);
}