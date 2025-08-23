using Himawari.VideoParser.Options;
using Himawari.VideoParser.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;
using Xunit;

namespace Himawari.VideoParser.Tests.Services;

[TestSubject(typeof(CobaltToolsVideoParser))]
public class CobaltToolsVideoParserTests : IClassFixture<CobaltToolsContainerFixture>
{
    private readonly CobaltToolsVideoParser _parser;

    public CobaltToolsVideoParserTests()
    {
        var client = new HttpClient();
        var options = new Mock<IOptions<VideoParsingOptions>>();
        options.SetupGet(o => o.Value).Returns(new VideoParsingOptions { CobaltToolsUrl = "http://localhost:9000" });
        _parser = new CobaltToolsVideoParser(client, options.Object, LoggerMock.Object);
        Task.Delay(2000).Wait();
    }

    [Theory]
    [MemberData(nameof(MatchData))]
    public void ContainsUrl_ShouldReturnExpectedValue(string url, bool shouldMatch) =>
        _parser.ContainsUrl(url).ShouldBe(shouldMatch);

    [Theory]
    [MemberData(nameof(ValidUrlsData))]
    public async Task GetInputFile_ShouldNotReturnNull_WhenUrlIsValid(string url)
    {
        var result = await _parser.GetInputFiles(url);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Errors.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(InvalidUrlsData))]
    public async Task GetInputFile_ShouldReturnNull_WhenUrlIsInvalid(string url)
    {
        var result = await _parser.GetInputFiles(url);
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

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
        "https://vm.tiktok.com/ZMBb5FeLg/",
        "https://www.tiktok.com/@bankai.games/video/7492817945027038486?_t=ZM-8vWPPtvHrcM&_r=1",
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

    private static readonly Mock<ILogger<CobaltToolsVideoParser>> LoggerMock = new Mock<ILogger<CobaltToolsVideoParser>>();
}