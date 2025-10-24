using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Himawari.CobaltTools.Models;

[PublicAPI]
public interface IPickerResponse
{
    PickerObject[]? Picker { get; init; }
    string? Audio { get; init; }
    string? AudioFilename { get; init; }

    [PublicAPI]
    sealed record PickerObject(PickerObject.PickerType Type, string Url, string? Thumb)
    {
        
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public enum PickerType { Photo , Video , Gif }
    }
}