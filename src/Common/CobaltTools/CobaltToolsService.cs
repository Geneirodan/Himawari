using System.Net.Http.Headers;
using System.Net.Http.Json;
using Himawari.CobaltTools.Models;
using Himawari.CobaltTools.Options;
using Microsoft.Extensions.Options;

namespace Himawari.CobaltTools;

public sealed class CobaltToolsService(HttpClient client, IOptions<CobaltToolsOptions> options) : ICobaltToolsService
{
    private readonly CobaltToolsOptions _options = options.Value;

    public async Task<CobaltToolsResponse?> DownloadAsync(string? url, CancellationToken token = default)
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
        var content = await response.Content.ReadFromJsonAsync<CobaltToolsResponse>(token).ConfigureAwait(false);
        return content;
    }
}