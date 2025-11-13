using System.Text.RegularExpressions;
using Ardalis.Result;
using Himawari.CobaltTools;
using Himawari.CobaltTools.Models;
using Himawari.VideoParser.Resources;
using Microsoft.Extensions.Logging;
using Serilog.Context;
using Telegram.Bot.Types;

namespace Himawari.VideoParser.Services;

public sealed partial class CobaltToolsVideoParser(
    HttpClient client,
    ICobaltToolsService service,
    ILogger<CobaltToolsVideoParser> logger
) : IVideoParser
{
    [GeneratedRegex("""
                    https:\/\/
                    (v(m|t)\.tiktok\.com\/(\w+)\/?)|
                    (www\.tiktok\.com\/@[^/]+\/video\/(\d+))|
                    ((www\.)?youtu(\.be|be\.com)\/)
                    """, RegexOptions.IgnorePatternWhitespace)]
    private static partial Regex UrlRegex { get; }

    public bool ContainsUrl(string url) => UrlRegex.IsMatch(url);
    public async Task<Result<IAlbumInputMedia[]>> GetInputFiles(string url, CancellationToken token = default)
    {
        if (!ContainsUrl(url))
            return Result.Error(Messages.InvalidUrl);
        try
        {
            var content = await service.DownloadAsync(url, token).ConfigureAwait(false);

            if (content is null)
                return Result.Error(Messages.DownloadFailed);

            switch (content.Status)
            {
                case Status.Tunnel or Status.Redirect when content is TunnelResponse response:
                    var video = await DownloadFileAsync(response.Url, response.Filename, token).ConfigureAwait(false);
                    return new[] { new InputMediaVideo(video) };

                case Status.Picker when content is PickerResponse response:
                    return await DownloadAllAsync(response, token).ConfigureAwait(false);

                case Status.Error when content is ErrorResponse response:
                    using (LogContext.PushProperty(nameof(response.Error), response.Error, destructureObjects: true))
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

        return Result.Error(Messages.DownloadFailed);
    }


    private async Task<IAlbumInputMedia[]> DownloadAllAsync(PickerResponse response, CancellationToken token = default)
    {
        var tasks = response.Picker.Select(x => DownloadSingleMedia(x.Url, x.Type, token: token)).ToList();
        if (response.Audio is not null)
            tasks.Add(DownloadSingleMedia(response.Audio, MediaType.Audio, response.AudioFilename, token));
        return await Task.WhenAll(tasks).ConfigureAwait(false);
    }

    private async Task<InputFileStream> DownloadFileAsync(
        string requestUri,
        string? filename = null,
        CancellationToken token = default
    )
    {
        var response = await client.GetAsync(requestUri, token).ConfigureAwait(false);
        var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
        return new InputFileStream(stream, filename);
    }

    private async Task<IAlbumInputMedia> DownloadSingleMedia(
        string url,
        MediaType mediaType,
        string? filename = null,
        CancellationToken token = default
    )
    {
        var (generator, extension) = ((Func<InputFile, IAlbumInputMedia>, string))
            (mediaType switch
            {
                MediaType.Photo => (x => new InputMediaPhoto(x), ".jpeg"),
                MediaType.Video => (x => new InputMediaVideo(x), ".mp4"),
                MediaType.Gif => (x => new InputMediaVideo(x), ".gif"),
                MediaType.Audio => (x => new InputMediaAudio(x), ".mp3"),
                _ => throw new InvalidOperationException()
            });
        var stream = await DownloadFileAsync(url, filename ?? Guid.CreateVersion7() + extension, token)
            .ConfigureAwait(false);
        return generator(stream);
    }
}