using JetBrains.Annotations;

namespace Himawari.VideoParser.CobaltTools;

[PublicAPI]
public interface ITunnelResponse
{
    public string? Url { get; init; }
    public string? Filename { get; init; }
}