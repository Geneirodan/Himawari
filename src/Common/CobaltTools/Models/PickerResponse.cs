using JetBrains.Annotations;
using static Himawari.CobaltTools.Models.PickerResponse;

namespace Himawari.CobaltTools.Models;

[PublicAPI]
public sealed record PickerResponse(
    PickerObject[] Picker,
    string? Audio = null,
    string? AudioFilename = null
) : CobaltToolsResponse
{
    public sealed record PickerObject(MediaType Type, string Url, string? Thumb);
}