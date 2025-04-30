using System.Text.RegularExpressions;
using Ardalis.Result;
using Himawari.VideoParser.Resources;
using Telegram.Bot.Types;

namespace Himawari.VideoParser.Services;

public partial class TikTokVideoParser(HttpClient client) : IVideoParser
{
    [GeneratedRegex(@"https:\/\/v(m|t)\.tiktok\.com\/(\w+)\/?")]
    private static partial Regex ShortUrlRegex { get; }

    [GeneratedRegex(@"https:\/\/www\.tiktok\.com\/@[^/]+\/video\/(\d+)")]
    private static partial Regex UrlRegex { get; }

    [GeneratedRegex(@"https://tikcdn.io/ssstik/(\d+)")]
    private static partial Regex DownloadUrlRegex { get; }

    public bool ContainsUrl(string url) => ShortUrlRegex.IsMatch(url) || UrlRegex.IsMatch(url);
    public string Type => "TikTok";

    public async Task<Result<InputFile>> GetInputFile(string url)
    {
        if (!ContainsUrl(url))
            return Result<InputFile>.Error(Messages.InvalidUrl);

        var content = new Dictionary<string, string>
        {
            { "id", url },
            { "locale", "en" },
            { "tt", "OGJyVWI_" }
        };
        const string requestUri = "https://ssstik.io/abc?url=dl";

        var retryCount = 3;
        
        do
        {
            SetRandomUserAgent();
            
            var formContent = new FormUrlEncodedContent(content);
            var response = await client.PostAsync(requestUri, formContent).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) 
                return Result<InputFile>.Error(Messages.Error);
            
            var html = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!DownloadUrlRegex.IsMatch(html))
            {
                await Task.Delay(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
                continue;
            }

            var downloadLink = DownloadUrlRegex.Match(html).Value;
            var video = await client.GetAsync(downloadLink).ConfigureAwait(false);
            if (!video.IsSuccessStatusCode)
                return Result<InputFile>.Error(Messages.DownloadFailed);
            
            var stream = await video.Content.ReadAsStreamAsync().ConfigureAwait(false);
            return new InputFileStream(stream, $"{Guid.CreateVersion7()}.mp4");
        } while (retryCount-- > 0);
        return Result<InputFile>.Error(Messages.DownloadFailed);
    }

    private readonly string[] _userAgents =
    [
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3",
        "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0.3 Safari/605.1.15",
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:87.0) Gecko/20100101 Firefox/87.0"
        // Add more as needed
    ];

    private void SetRandomUserAgent()
    {
        var randomAgent = _userAgents[Random.Shared.Next(_userAgents.Length)];
        client.DefaultRequestHeaders.UserAgent.ParseAdd(randomAgent);
    }
}