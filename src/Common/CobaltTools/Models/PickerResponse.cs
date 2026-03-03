using JetBrains.Annotations;
using static Himawari.CobaltTools.Models.PickerResponse;

namespace Himawari.CobaltTools.Models;

/// <summary>
/// CobaltTools API response when the user must pick from multiple options (e.g. format/quality). Contains picker entries and optional audio URLs.
/// </summary>
/// <param name="Picker">Array of options (type, URL, optional thumb).</param>
/// <param name="Audio">Optional direct audio URL.</param>
/// <param name="AudioFilename">Optional filename for the audio.</param>
[PublicAPI]
public sealed record PickerResponse(
    PickerObject[] Picker,
    string? Audio = null,
    string? AudioFilename = null
) : CobaltToolsResponse
{
    /// <summary>
    /// A single picker option: media type, URL, and optional thumbnail URL.
    /// </summary>
    /// <param name="Type">Kind of media (e.g. <see cref="MediaType.Video"/>, <see cref="MediaType.Audio"/>).</param>
    /// <param name="Url">URL for this option.</param>
    /// <param name="Thumb">Optional thumbnail URL.</param>
    public sealed record PickerObject(MediaType Type, string Url, string? Thumb);
}