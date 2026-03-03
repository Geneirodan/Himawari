using System.Text.Json.Serialization;
using JetBrains.Annotations;

namespace Himawari.CobaltTools.Models;

/// <summary>
/// Contract for a CobaltTools API response that exposes a <see cref="Status"/>.
/// </summary>
[PublicAPI]
public interface ICobaltToolsResponse
{
    /// <summary>
    /// Result status of the request (e.g. tunnel, local-processing, redirect, picker, error).
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    Status Status { get; init; }
}