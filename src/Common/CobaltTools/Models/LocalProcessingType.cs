using JetBrains.Annotations;

namespace Himawari.CobaltTools.Models;

/// <summary>
/// CobaltTools local processing mode (merge streams, mute, extract audio, gif, remux).
/// </summary>
[PublicAPI]
public enum LocalProcessingType
{
    /// <summary>Merge video and audio streams.</summary>
    Merge,
    /// <summary>Mute video (audio removed).</summary>
    Mute,
    /// <summary>Extract audio only.</summary>
    Audio,
    /// <summary>Convert to GIF.</summary>
    Gif,
    /// <summary>Remux without re-encoding.</summary>
    Remux
}