using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Himawari.CobaltTools.Models;

[PublicAPI]
public interface ICobaltToolsResponse
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    Status Status { get; init; }
}