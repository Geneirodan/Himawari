using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Himawari.VideoParser.CobaltTools;

[PublicAPI]
public interface IPickerResponse
{
    public PickerObject[]? Picker { get; init; }
    public string? Audio { get; init; }
    public string? AudioFilename { get; init; }

    [PublicAPI]
    public sealed record PickerObject(PickerObject.PickerType Type, string Url, string? Thumb)
    {
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum PickerType { Photo , Video , Gif }
    }
}