using JetBrains.Annotations;

namespace Himawari.CobaltTools.Models;

[PublicAPI]
public interface ITunnelResponse
{
    string? Url { get; init; }
    string? Filename { get; init; }
}