using Himawari.CobaltTools.Models;

namespace Himawari.CobaltTools;

public interface ICobaltToolsService
{
    Task<CobaltToolsResponse?> DownloadAsync(string? url, CancellationToken token = default);
}