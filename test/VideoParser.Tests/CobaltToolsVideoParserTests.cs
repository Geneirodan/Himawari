using Himawari.CobaltTools;
using Himawari.CobaltTools.Models;
using Himawari.VideoParser.Services;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using Xunit;

namespace Himawari.VideoParser.Tests;

[TestSubject(typeof(CobaltToolsVideoParser)), UsedImplicitly]
public sealed class CobaltToolsVideoParserTests
{
    private readonly CobaltToolsVideoParser _parser;
    private readonly Mock<ICobaltToolsService> _cobaltToolsService = new();
    private readonly FakeHttpMessageHandler _fakeHttpMessageHandler = new();

    public CobaltToolsVideoParserTests()
    {
        var client = new HttpClient(_fakeHttpMessageHandler);
        _parser = new CobaltToolsVideoParser(client, _cobaltToolsService.Object, LoggerMock.Object);
    }

    [Theory]
    [MemberData(nameof(MatchData))]
    public void ContainsUrl_ShouldReturnExpectedValue(string url, bool shouldMatch) =>
        _parser.ContainsUrl(url).ShouldBe(shouldMatch);

    [Theory(Skip = "Cobalt tools currently does not work with YT")]
    [InlineData(Status.Tunnel)]
    [InlineData(Status.Redirect)]
    public async Task GetInputFiles_ShouldReturnVideo_WhenCobaltToolsReturnedTunnelOrRedirectStatus(Status status)
    {
        _cobaltToolsService.Setup(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TunnelResponse("http://new_url","video.mp4"){Status = status});
        _fakeHttpMessageHandler.Handler = _ => new HttpResponseMessage
        {
            Content = new StringContent("FileContent")
        };
        var result = await _parser.GetInputFiles("https://www.youtube.com/shorts/T0t-DYPWVw0", TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Errors.ShouldBeEmpty();
        result.Value.Length.ShouldBe(1);
    }

    [Fact]
    public async Task GetInputFiles_ShouldReturnMultipleMedia_WhenCobaltToolsReturnedPickerStatus()
    {
        _cobaltToolsService.Setup(x => x.DownloadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PickerResponse([
                new PickerResponse.PickerObject(MediaType.Photo, "http://url1", Thumb: null),
                new PickerResponse.PickerObject(MediaType.Gif, "http://url2", Thumb: null),
                new PickerResponse.PickerObject(MediaType.Video, "http://url3", Thumb: null)
            ])
            {
                Status = Status.Picker
            });
        _fakeHttpMessageHandler.Handler = _ => new HttpResponseMessage
        {
            Content = new StringContent("FileContent")
        };
        var result = await _parser.GetInputFiles("https://www.youtube.com/shorts/T0t-DYPWVw0",
            TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Errors.ShouldBeEmpty();
        result.Value.Length.ShouldBe(3);
    }

    [Theory]
    [MemberData(nameof(InvalidUrlsData))]
    public async Task GetInputFile_ShouldReturnNull_WhenUrlIsInvalid(string url)
    {
        var result = await _parser.GetInputFiles(url, TestContext.Current.CancellationToken);
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    public static TheoryData<string, bool> MatchData()
    {
        var td = new TheoryData<string, bool>();
        foreach (var url in ValidUrls) td.Add(url, p2: true);
        foreach (var url in InvalidUrls) td.Add(url, p2: false);
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

    private static readonly Mock<ILogger<CobaltToolsVideoParser>> LoggerMock = new();
}