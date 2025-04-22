using System.Text.RegularExpressions;
using Telegram.Bot.Types;
using YoutubeExplode;
using YoutubeExplode.Converter;

namespace Himawari.VideoParser.Services;

public partial class YouTubeVideoParser(HttpClient httpClient) : IVideoParser
{
    [GeneratedRegex(@"https:\/\/(www\.)?youtu(\.be|be\.com)\/")]
    private static partial Regex UrlRegex { get; }

    public async Task<InputFile?> GetInputFile(string url)
    {
        if (!ContainsUrl(url))
            return null;
        var youtube = new YoutubeClient(httpClient);
        var video = await youtube.Videos.GetAsync(url).ConfigureAwait(false);
        var sanitizedTitle = string.Join("_", video.Title.Split(Path.GetInvalidFileNameChars()));
        var outputFilePath = $"{sanitizedTitle}.mp4";
        await youtube.Videos.DownloadAsync(url, outputFilePath).ConfigureAwait(false);
        var stream = new FileStream(
            path: outputFilePath,
            mode: FileMode.Open,
            access: FileAccess.ReadWrite,
            share: FileShare.Read,
            bufferSize: 4096,
            options: FileOptions.DeleteOnClose
        );
        return new InputFileStream(stream);
    }

    public bool ContainsUrl(string url) => UrlRegex.IsMatch(url);

    public string Type => "YouTube";
}