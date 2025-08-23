using JetBrains.Annotations;

namespace Himawari.VideoParser.CobaltTools;

[PublicAPI]
public enum LocalProcessingType
{
    Merge,
    Mute,
    Audio,
    Gif,
    Remux
}