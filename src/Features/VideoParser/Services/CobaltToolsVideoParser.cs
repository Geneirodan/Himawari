using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Ardalis.Result;
using Himawari.VideoParser.CobaltTools;
using Himawari.VideoParser.Options;
using Himawari.VideoParser.Resources;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;
using Telegram.Bot.Types;
using static Himawari.VideoParser.CobaltTools.IPickerResponse;
using static Himawari.VideoParser.CobaltTools.IPickerResponse.PickerObject;

namespace Himawari.VideoParser.Services;

public partial class CobaltToolsVideoParser(
    HttpClient client,
    IOptions<VideoParsingOptions> options,
    ILogger<CobaltToolsVideoParser> logger
) : IVideoParser
{
    private readonly VideoParsingOptions _options = options.Value;

    [GeneratedRegex("""
                    https:\/\/
                    (v(m|t)\.tiktok\.com\/(\w+)\/?)|
                    (www\.tiktok\.com\/@[^/]+\/video\/(\d+))|
                    ((www\.)?youtu(\.be|be\.com)\/)
                    """, RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex UrlRegex { get; }

    public bool ContainsUrl(string url) => UrlRegex.IsMatch(url);
    public string Type => "TikTok";

    public async Task<Result<IAlbumInputMedia[]>> GetInputFiles(string url, CancellationToken token = default)
    {
        if (!ContainsUrl(url))
            return Result<IAlbumInputMedia[]>.Error(Messages.InvalidUrl);
        try
        {
            var jsonContent = JsonContent.Create(new { url });
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var message = new HttpRequestMessage
            {
                Content = jsonContent,
                Method = HttpMethod.Post,
                RequestUri = new Uri(_options.CobaltToolsUrl)
            };
            message.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await client.SendAsync(message, token).ConfigureAwait(false);
            var content = await response.Content.ReadFromJsonAsync<CobaltToolsResponse>(token).ConfigureAwait(false);

            if (content is null)
                return Result<IAlbumInputMedia[]>.Error(Messages.DownloadFailed);

            switch (content.Status)
            {
                case Status.Tunnel when content.Url is not null:
                case Status.Redirect when content.Url is not null:
                    var video = await DownloadVideoAsync(content.Url, content.Filename, token).ConfigureAwait(false);
                    return new[] { new InputMediaVideo(video) };

                case Status.Picker when content.Picker is not null:
                    return await DownloadAllAsync(content.Picker, token).ConfigureAwait(false);

                case Status.Error:
                    using (LogContext.PushProperty(nameof(IErrorResponse.Error), content.Error, true))
                        logger.LogError("CobaltTools error occured");
                    break;

                case Status.LocalProcessing:
                default:
                    logger.LogError("Unsupported status {Status}", content.Status);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "CobaltTools error occured");
        }

        return Result<IAlbumInputMedia[]>.Error(Messages.DownloadFailed);
    }

    private async Task<IAlbumInputMedia[]> DownloadAllAsync(PickerObject[] pickerObjects,
        CancellationToken token = default) =>
        await Task.WhenAll(pickerObjects.Select(x => DownloadSingleMedia(x, token))).ConfigureAwait(false);

    private async Task<InputFileStream> DownloadVideoAsync(
        string requestUri,
        string? filename = null,
        CancellationToken token = default
    )
    {
        var response = await client.GetAsync(requestUri, token).ConfigureAwait(false);
        var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
        return new InputFileStream(stream, filename);
    }

    private async Task<IAlbumInputMedia> DownloadSingleMedia(PickerObject picker, CancellationToken token = default)
    {
        var response = await client.GetAsync(picker.Url, token).ConfigureAwait(false);
        var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
        return picker.Type switch
        {
            PickerType.Photo => new InputMediaPhoto(
                new InputFileStream(stream, $"{Guid.CreateVersion7()}.jpeg")
            ),
            PickerType.Video => new InputMediaVideo(
                new InputFileStream(stream, $"{Guid.CreateVersion7()}.mp4")
            ),
            PickerType.Gif => new InputMediaVideo(
                new InputFileStream(stream, $"{Guid.CreateVersion7()}.gif")
            ),
            _ => throw new InvalidOperationException()
        };
    }
}