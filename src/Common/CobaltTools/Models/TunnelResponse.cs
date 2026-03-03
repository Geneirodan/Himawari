using JetBrains.Annotations;

namespace Himawari.CobaltTools.Models;

/// <summary>
/// CobaltTools API response when media is delivered via a tunnel: stream URL and filename.
/// </summary>
/// <param name="Url">URL to stream or download the media.</param>
/// <param name="Filename">Suggested filename for the media.</param>
[PublicAPI]
public sealed record TunnelResponse(string Url, string Filename) : CobaltToolsResponse;