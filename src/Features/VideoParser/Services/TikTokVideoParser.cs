using System.Text.Json;
using System.Text.RegularExpressions;
using Telegram.Bot.Types;

namespace Himawari.VideoParser.Services;

public partial class TikTokVideoParser(HttpClient client) : IVideoParser
{
    [GeneratedRegex(@"https:\/\/vm\.tiktok\.com\/(\w+)\/")]
    private static partial Regex ShortUrlRegex { get; }

    [GeneratedRegex(@"https:\/\/www\.tiktok\.com\/@[^/]+\/video\/(\d+)")]
    private static partial Regex UrlRegex { get; }

    public bool ContainsUrl(string url) => ShortUrlRegex.IsMatch(url) || UrlRegex.IsMatch(url);
    public string Type => "TikTok";

    public async Task<InputFile?> GetInputFile(string url)
    {
        var fullUrl = url;
        SetRandomUserAgent();
        if (ShortUrlRegex.Match(url) is { Success: true } shortMatch)
        {
            var response = await client.GetAsync(shortMatch.Groups[0].Value).ConfigureAwait(false);
            if (response.IsSuccessStatusCode)
            {
                fullUrl = response.RequestMessage?.RequestUri?.ToString();
                if(fullUrl is null)
                    return null;
            }
            else
                return null;
        }

        var match = UrlRegex.Match(fullUrl);
        if (!match.Success)
            return null;
        var videoId = match.Groups[1].Value;

        var newUrl = await GetDownloadLinkAsync(videoId).ConfigureAwait(false);
        return newUrl is null ? null : new InputFileUrl(newUrl);
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

    private async Task<string?> GetDownloadLinkAsync(string? videoId)
    {
        var apiUrl = $"https://api22-normal-c-alisg.tiktokv.com/aweme/v1/feed/" +
                     $"?aweme_id={videoId}" +
                     $"&iid=7318518857994389254" +
                     $"&device_id=7318517321748022790" +
                     $"&channel=googleplay" +
                     $"&app_name=musical_ly" +
                     $"&version_code=300904" +
                     $"&device_platform=android" +
                     $"&device_type=ASUS_Z01QD" +
                     $"&version=9";

        var response = await client.GetAsync(apiUrl).ConfigureAwait(false);
        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

        return response.IsSuccessStatusCode ? ExtractDownloadUrl(content) : null;
    }

    private static string? ExtractDownloadUrl(string apiResponse)
    {
        using var doc = JsonDocument.Parse(apiResponse);
        return doc.RootElement
            .GetProperty("aweme_list")[0]
            .GetProperty("video")
            .GetProperty("play_addr")
            .GetProperty("url_list")[0]
            .GetString()?
            .Replace("\\u0026", "&");
    }
}