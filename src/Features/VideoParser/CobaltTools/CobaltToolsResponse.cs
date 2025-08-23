using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Himawari.VideoParser.CobaltTools;

[PublicAPI]
public sealed record CobaltToolsResponse : ITunnelResponse, IPickerResponse, ILocalProcessingResponse, IErrorResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Status Status { get; init; }
    ILocalProcessingResponse.AudioObject? ILocalProcessingResponse.Audio { get; init; }

    string? IPickerResponse.Audio { get; init; }

    public string? Url { get; init; }
    public string? Filename { get; init; }
    public IPickerResponse.PickerObject[]? Picker { get; init; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public LocalProcessingType? Type { get; init; }
    public string? Service { get; init; }
    public string[]? Tunnel { get; init; }
    public ILocalProcessingResponse.OutputObject? Output { get; init; }
    public bool? IsHls { get; init; }
    public string? AudioFilename { get; init; }
    public IErrorResponse.ErrorObject? Error { get; init; }
}