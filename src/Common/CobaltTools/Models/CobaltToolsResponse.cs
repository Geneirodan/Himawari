using System.Text.Json.Serialization;

namespace Himawari.CobaltTools.Models;

/// <summary>
/// Base response from the CobaltTools API with a <see cref="Status"/> value.
/// </summary>
public record CobaltToolsResponse : ICobaltToolsResponse
{
    /// <inheritdoc />
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public required Status Status { get; init; }
}