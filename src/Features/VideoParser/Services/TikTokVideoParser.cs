using System.Net;
using System.Text.RegularExpressions;
using Ardalis.Result;
using Himawari.VideoParser.Resources;
using Polly;
using Polly.Retry;
using Telegram.Bot.Types;

namespace Himawari.VideoParser.Services;

public partial class TikTokVideoParser(HttpClient client) : IVideoParser
{
    private const string? _tiktokioUrl = "https://tiktokio.com";
    private const string ReqUri = "https://tiktokio.com/api/v1/tk-htmx";

    private static readonly AsyncRetryPolicy<HttpResponseMessage> PolicyForNoContent = Policy
        .HandleResult<HttpResponseMessage>(x => x.StatusCode == HttpStatusCode.NoContent)
        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    private static readonly AsyncRetryPolicy<HttpResponseMessage> PolicyForRetries = Policy
        .HandleResult<HttpResponseMessage>(x => x.Content.Headers.ContentLength is null or 0)
        .WaitAndRetryAsync(5, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

    [GeneratedRegex(@"https:\/\/v(m|t)\.tiktok\.com\/(\w+)\/?")]
    private static partial Regex ShortUrlRegex { get; }

    [GeneratedRegex(@"https:\/\/www\.tiktok\.com\/@[^/]+\/video\/(\d+)")]
    private static partial Regex UrlRegex { get; }

    [GeneratedRegex("""<input type="hidden" name="prefix" value="(.+?)"/>""")]
    private static partial Regex PrefixRegex { get; }

    [GeneratedRegex("""<a href="(.+?)".*?>Download without watermark</a>""")]
    private static partial Regex DownloadVideoUrlRegex { get; }

    [GeneratedRegex("""<a href="(.+?)".*?> Download Photo </a>""")]
    private static partial Regex DownloadSlidesUrlRegex { get; }

    public bool ContainsUrl(string url) => ShortUrlRegex.IsMatch(url) || UrlRegex.IsMatch(url);
    public string Type => "TikTok";

    public async Task<Result<IAlbumInputMedia[]>> GetInputFiles(string url)
    {
        if (!ContainsUrl(url))
            return Result<IAlbumInputMedia[]>.Error(Messages.InvalidUrl);

        var prefix = await GetPrefixAsync().ConfigureAwait(false);

        if (prefix is null)
            return Result<IAlbumInputMedia[]>.Error(string.Format(Messages.Error, url));

        var html = await GetContentAsync(url, prefix).ConfigureAwait(false);


        if (DownloadVideoUrlRegex.Match(html) is { Success: true } match)
        {
            var file = await DownloadVideoAsync(match.Groups[1].Value).ConfigureAwait(false);

            if (file is not null)
                return new[] { new InputMediaVideo(file) };
        }

        if (DownloadSlidesUrlRegex.Matches(html) is { Count: > 0 } matches)
        {
            var photos = await DownloadPhotosAsync(matches.Select(x => x.Groups[1].Value)).ConfigureAwait(false);
            if (photos is not null)
                return photos;
        }

        return Result<IAlbumInputMedia[]>.Error(Messages.DownloadFailed);
    }

    private async Task<InputMediaPhoto[]?> DownloadPhotosAsync(params IEnumerable<string> urls)
    {
        var tasks = urls.Select(DownloadPhoto);
        var results = await Task.WhenAll(tasks).ConfigureAwait(false);
        return results.Any(x => x is null) 
            ? null 
            : results.Select(x =>new InputMediaPhoto(x!)).ToArray();
    }

    private async Task<InputFileStream?> DownloadVideoAsync(string downloadLink)
    {
        var video = await client.GetAsync(downloadLink).ConfigureAwait(false);
        if (!video.IsSuccessStatusCode)
            return null;

        var stream = await video.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return new InputFileStream(stream, $"{Guid.CreateVersion7()}.mp4");
    }

    private async Task<string> GetContentAsync(string url, string prefix)
    {
        var dictionary = new Dictionary<string, string> { { "prefix", prefix }, { "vid", url } };
        var content = new FormUrlEncodedContent(dictionary);
        var response = await PolicyForRetries.ExecuteAsync(() => GetPage(ReqUri, content)).ConfigureAwait(false);
        var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        return html;
    }

    private async Task<string?> GetPrefixAsync()
    {
        var site = await PolicyForNoContent.ExecuteAsync(() => client.GetAsync(_tiktokioUrl)).ConfigureAwait(false);

        var html = await site.Content.ReadAsStringAsync().ConfigureAwait(false);
        return PrefixRegex.Match(html) is { Success: true } match
            ? match.Groups[1].Value
            : null;
    }

    private async Task<HttpResponseMessage> GetPage(string requestUri, FormUrlEncodedContent formContent)
    {
        SetRandomUserAgent();
        return await client.PostAsync(requestUri, formContent).ConfigureAwait(false);
    }

    private async Task<InputFileStream?> DownloadPhoto(string requestUri)
    {
        var photo = await PolicyForNoContent.ExecuteAsync(() => GetPhoto(requestUri)).ConfigureAwait(false);
        if (photo.StatusCode != HttpStatusCode.OK) return null;

        var stream = await photo.Content.ReadAsStreamAsync().ConfigureAwait(false);
        return new InputFileStream(stream, $"{Guid.CreateVersion7()}.jpeg");
    }

    private async Task<HttpResponseMessage> GetPhoto(string requestUri)
    {
        using var httpMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
        return await client.SendAsync(httpMessage).ConfigureAwait(false);
    }

    private readonly string[] _userAgents =
    [
        // "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3",
        // "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0.3 Safari/605.1.15",
        // "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0"
        // Add more as needed
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:141.0) Gecko/20100101 Firefox/141.0"
    ];

    private void SetRandomUserAgent()
    {
        var randomAgent = _userAgents[Random.Shared.Next(_userAgents.Length)];
        client.DefaultRequestHeaders.UserAgent.ParseAdd(randomAgent);
    }
}