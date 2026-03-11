using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Himawari.CobaltTools.Models;
using Himawari.CobaltTools.Options;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Himawari.CobaltTools;

/// <summary>
/// HTTP client implementation of <see cref="ICobaltToolsService"/> that POSTs a URL to the CobaltTools API and deserializes the response by <see cref="Status"/>. Uses <see cref="HybridCache"/> to avoid duplicate requests for the same URL (stampede protection).
/// </summary>
public sealed class CobaltToolsService(
    HttpClient client,
    HybridCache cache,
    IOptions<CobaltToolsOptions> options,
    ILogger<CobaltToolsService> logger) : ICobaltToolsService
{
    private readonly CobaltToolsOptions _options = options.Value;
    private readonly JsonSerializerOptions _jsonSerializerOptions = JsonSerializerOptions.Web;

    /// <inheritdoc />
    public async Task<ICobaltToolsResponse?> DownloadAsync(string? url, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(url))
            return null;

        var cacheKey = $"cobalt:{NormalizeUrl(url)}";

        return await cache.GetOrCreateAsync(
            cacheKey,
            async innerCt =>
            {
                logger.LogDebug("Cache miss for {Url}, fetching from CobaltTools", url);
                return await FetchFromApiAsync(url, innerCt).ConfigureAwait(false);
            },
            new HybridCacheEntryOptions { Expiration = TimeSpan.FromMinutes(30) },
            tags: null,
            cancellationToken: token).ConfigureAwait(false);
    }

    /// <summary>
    /// Normalizes <paramref name="url"/> for cache keying: lowercases scheme/host, removes UTM parameters and trailing slashes.
    /// </summary>
    private static string NormalizeUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            return url.ToLowerInvariant();

        var query = uri.Query;
        if (!string.IsNullOrEmpty(query))
        {
            var pairs = query.TrimStart('?').Split('&');
            var filtered = pairs.Where(p =>
                !p.StartsWith("utm_", StringComparison.OrdinalIgnoreCase) &&
                !p.StartsWith("utm-", StringComparison.OrdinalIgnoreCase));
            query = string.Join("&", filtered);
        }

        var builder = new UriBuilder(uri) { Query = query };
        return builder.Uri.ToString().TrimEnd('/').ToLowerInvariant();
    }

    private async Task<ICobaltToolsResponse?> FetchFromApiAsync(string url, CancellationToken token)
    {
        var jsonContent = JsonContent.Create(new { url });
        jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var message = new HttpRequestMessage
        {
            Content = jsonContent,
            Method = HttpMethod.Post,
            RequestUri = new Uri(_options.Url)
        };
        message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        var response = await client.SendAsync(message, token).ConfigureAwait(false);
        var json = await response.Content.ReadAsStringAsync(token).ConfigureAwait(false);
        return JsonSerializer.Deserialize<CobaltToolsResponse>(json, _jsonSerializerOptions)?.Status switch
        {
            Status.Tunnel or Status.Redirect =>
                JsonSerializer.Deserialize<TunnelResponse>(json, _jsonSerializerOptions),
            Status.LocalProcessing => JsonSerializer.Deserialize<LocalProcessingResponse>(json, _jsonSerializerOptions),
            Status.Picker => JsonSerializer.Deserialize<PickerResponse>(json, _jsonSerializerOptions),
            Status.Error => JsonSerializer.Deserialize<ErrorResponse>(json, _jsonSerializerOptions),
            _ => null
        };
    }
}
