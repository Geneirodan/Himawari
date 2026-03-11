using Himawari.CobaltTools.Models;

namespace Himawari.CobaltTools;

/// <summary>
/// Service that downloads or processes media via the CobaltTools API.
/// </summary>
public interface ICobaltToolsService
{
    /// <summary>
    /// Requests the CobaltTools API for the given URL and returns the response, or <see langword="null"/> on failure.
    /// </summary>
    /// <param name="url">Source URL to process (e.g. video or audio link).</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The result is an <see cref="ICobaltToolsResponse"/> or <see langword="null"/>.</returns>
    Task<ICobaltToolsResponse?> DownloadAsync(string? url, CancellationToken token = default);
}