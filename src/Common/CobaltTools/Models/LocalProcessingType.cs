using JetBrains.Annotations;

namespace Himawari.CobaltTools.Models;

[PublicAPI]
public enum LocalProcessingType
{
    Merge,
    Mute,
    Audio,
    Gif,
    Remux
}