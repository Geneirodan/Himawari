using Himawari.VideoParser.Services;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

namespace Himawari.VideoParser.Tests.Services;

#if DEBUG
[TestSubject(typeof(YouTubeVideoParser))]
public class YouTubeVideoParserTests
{
    private readonly YouTubeVideoParser _parser;

    public YouTubeVideoParserTests()
    {
        var client = new HttpClient();
        _parser = new YouTubeVideoParser(client);
    }

    [Theory]
    [MemberData(nameof(MatchData))]
    public void ContainsUrl_ShouldReturnExpectedValue(string url, bool shouldMatch) =>
        _parser.ContainsUrl(url).ShouldBe(shouldMatch);

    [Theory]
    [MemberData(nameof(ValidUrlsData))]
    public async Task GetInputFile_ShouldNotReturnNull_WhenUrlIsValid(string url) =>
        (await _parser.GetInputFile(url)).ShouldNotBeNull();

    [Theory]
    [MemberData(nameof(InvalidUrlsData))]
    public async Task GetInputFile_ShouldReturnNull_WhenUrlIsInvalid(string url) =>
        (await _parser.GetInputFile(url)).ShouldBeNull();

    public static TheoryData<string, bool> MatchData()
    {
        var td = new TheoryData<string, bool>();
        foreach (var url in ValidUrls) td.Add(url, true);
        foreach (var url in InvalidUrls) td.Add(url, false);
        return td;
    }

    public static TheoryData<string> ValidUrlsData()
    {
        var td = new TheoryData<string>();
        foreach (var url in ValidUrls) td.Add(url);
        return td;
    }

    public static TheoryData<string> InvalidUrlsData()
    {
        var td = new TheoryData<string>();
        foreach (var url in InvalidUrls) td.Add(url);
        return td;
    }

    private static readonly string[] ValidUrls =
    [
        "https://www.youtube.com/shorts/T0t-DYPWVw0",
        "https://youtube.com/shorts/T0t-DYPWVw0?si=zAXrdjE2N12lZru6",
        "https://www.youtube.com/watch?v=oVWEb-At8yc",
        "https://youtu.be/oVWEb-At8yc?si=B-njYVy3SsHNIrUX",
        "https://www.youtube.com/embed/oVWEb-At8yc"
    ]; 

    private static readonly string[] InvalidUrls =
    [
        "https://vm.tiktok.com/",
        "https://www.tiktok.com/video/",
        "somesort of somtsf"
    ];
}
# endif