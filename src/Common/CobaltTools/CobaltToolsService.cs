using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Himawari.CobaltTools.Models;
using Himawari.CobaltTools.Options;
using Microsoft.Extensions.Options;

namespace Himawari.CobaltTools;

public sealed class CobaltToolsService(HttpClient client, IOptions<CobaltToolsOptions> options) : ICobaltToolsService
{
    private readonly CobaltToolsOptions _options = options.Value;
    private readonly JsonSerializerOptions _jsonSerializerOptions = JsonSerializerOptions.Web;

    public async Task<ICobaltToolsResponse?> DownloadAsync(string? url, CancellationToken token = default)
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