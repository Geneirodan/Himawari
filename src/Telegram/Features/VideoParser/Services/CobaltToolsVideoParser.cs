using System.Text.RegularExpressions;
using Ardalis.Result;
using Himawari.CobaltTools;
using Himawari.CobaltTools.Models;
using Himawari.VideoParser.Resources;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Telegram.Bot.Types;
using static Himawari.CobaltTools.Models.IPickerResponse;

namespace Himawari.VideoParser.Services;

public sealed partial class CobaltToolsVideoParser(HttpClient client, ICobaltToolsService service, ILogger<CobaltToolsVideoParser> logger) : IVideoParser
{

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
            var content = await service.DownloadAsync(url, token).ConfigureAwait(false);

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
                    using (LogContext.PushProperty(nameof(content.Error), content.Error, destructureObjects: true))
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
            PickerObject.PickerType.Photo => new InputMediaPhoto(
                new InputFileStream(stream, $"{Guid.CreateVersion7()}.jpeg")
            ),
            PickerObject.PickerType.Video => new InputMediaVideo(
                new InputFileStream(stream, $"{Guid.CreateVersion7()}.mp4")
            ),
            PickerObject.PickerType.Gif => new InputMediaVideo(
                new InputFileStream(stream, $"{Guid.CreateVersion7()}.gif")
            ),
            _ => throw new InvalidOperationException()
        };
    }
}