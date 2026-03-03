using System.Text.Json.Serialization;

namespace Himawari.CobaltTools.Models;

/// <summary>
/// Type of media supported by CobaltTools (photo, video, gif, audio).
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MediaType
{
    /// <summary>Photo/image.</summary>
    Photo,
    /// <summary>Video.</summary>
    Video,
    /// <summary>Animated GIF.</summary>
    Gif,
    /// <summary>Audio.</summary>
    Audio
}