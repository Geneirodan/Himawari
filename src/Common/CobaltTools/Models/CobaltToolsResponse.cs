using System.Text.Json.Serialization;

namespace Himawari.CobaltTools.Models;

public record CobaltToolsResponse : ICobaltToolsResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Status Status { get; init; }
}