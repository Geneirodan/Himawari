using JetBrains.Annotations;
using static Himawari.CobaltTools.Models.PickerResponse;

namespace Himawari.CobaltTools.Models;

[PublicAPI]
public sealed record PickerResponse(PickerObject[] Picker, string? Audio = null, string? AudioFilename = null) : CobaltToolsResponse
{
    public override Status Status { get; init; } = Status.Picker;
    public sealed record PickerObject(MediaType Type, string Url, string? Thumb);
}