using JetBrains.Annotations;

namespace Himawari.CobaltTools.Models;

[PublicAPI]
public sealed record TunnelResponse(string Url, string Filename) : CobaltToolsResponse
{
    public override Status Status { get; init; } = Status.Picker;
}