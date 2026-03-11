namespace Himawari.CobaltTools.Options;

/// <summary>
/// Configuration for the CobaltTools API client: base URL of the API endpoint.
/// </summary>
public sealed record CobaltToolsOptions
{
    /// <summary>Base URL of the CobaltTools API (e.g. https://api.cobalt.tools/).</summary>
    public required string Url { get; init; }
}