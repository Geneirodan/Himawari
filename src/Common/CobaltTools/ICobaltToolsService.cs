using Himawari.CobaltTools.Models;

namespace Himawari.CobaltTools;

public interface ICobaltToolsService
{
    Task<ICobaltToolsResponse?> DownloadAsync(string? url, CancellationToken token = default);
}