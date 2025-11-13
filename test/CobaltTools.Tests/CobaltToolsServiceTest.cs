using Himawari.CobaltTools.Models;
using Himawari.CobaltTools.Options;
using Xunit;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Moq;
using Shouldly;

namespace Himawari.CobaltTools.Tests;

#if DEBUG
[TestSubject(typeof(CobaltToolsService))]
public sealed class CobaltToolsServiceTest : IClassFixture<CobaltToolsContainerFixture>
{
    private readonly CobaltToolsService _parser;

    public CobaltToolsServiceTest(CobaltToolsContainerFixture fixture)
    {
            var client = new HttpClient();
            var options = new Mock<IOptions<CobaltToolsOptions>>();
            var cobaltToolsOptions = new CobaltToolsOptions { Url = $"https://{fixture.Container.Hostname}:9000" };
            options.SetupGet(o => o.Value).Returns(cobaltToolsOptions);
            _parser = new CobaltToolsService(client, options.Object);
    }
    
    [Theory]
    [MemberData(nameof(TikTokVideoUrlsData))]
    public async Task DownloadAsync_ShouldReturnTunnelResponse_WhenUrlIsTikTokVideoUrl(string url)
    {
        var result = await _parser.DownloadAsync(url, TestContext.Current.CancellationToken);
        result.ShouldNotBeNull();
        result.Status.ShouldBe(Status.Tunnel);
    }
    [Theory]
    [MemberData(nameof(TikTokPhotoUrlsData))]
    public async Task DownloadAsync_ShouldReturnPickerResponse_WhenUrlIsTikTokPhotoUrl(string url)
    {
        var result = await _parser.DownloadAsync(url, TestContext.Current.CancellationToken);
        result.ShouldNotBeNull();
        result.Status.ShouldBe(Status.Picker);
        var picker = result as PickerResponse;
        picker.ShouldNotBeNull();
        picker.Picker.ShouldNotBeNull();
        picker.Picker.Length.ShouldBeGreaterThan(0);
    }
    [Theory]
    [MemberData(nameof(YouTubeUrlsData))]
    public async Task DownloadAsync_ShouldReturnTunnelResponse_WhenUrlIsYoutubePhotoUrl(string url)
    {
        var result = await _parser.DownloadAsync(url, TestContext.Current.CancellationToken);
        result.ShouldNotBeNull();
        result.Status.ShouldBe(Status.Tunnel);
    }

    [Theory]
    [MemberData(nameof(InvalidUrlsData))]
    public async Task GetInputFile_ShouldReturnError_WhenUrlIsInvalid(string url)
    {
        var result = await _parser.DownloadAsync(url, TestContext.Current.CancellationToken);
        result.ShouldNotBeNull();
        result.Status.ShouldBe(Status.Error);
    }

    public static TheoryData<string> TikTokVideoUrlsData()
    {
        var td = new TheoryData<string>();
        foreach (var url in TikTokVideoUrls) td.Add(url);
        return td;
    }
    public static TheoryData<string> TikTokPhotoUrlsData()
    {
        var td = new TheoryData<string>();
        foreach (var url in TikTokPhotoUrls) td.Add(url);
        return td;
    }
    public static TheoryData<string> YouTubeUrlsData()
    {
        var td = new TheoryData<string>();
        foreach (var url in YouTubeUrls) td.Add(url);
        return td;
    }

    public static TheoryData<string> InvalidUrlsData()
    {
        var td = new TheoryData<string>();
        foreach (var url in InvalidUrls) td.Add(url);
        return td;
    }

    private static readonly string[] TikTokVideoUrls =
    [
        "https://vm.tiktok.com/ZMBb5FeLg/",
        "https://www.tiktok.com/@bankai.games/video/7492817945027038486?_t=ZM-8vWPPtvHrcM&_r=1"
    ];
    private static readonly string[] TikTokPhotoUrls =
    [
        "https://vm.tiktok.com/ZMSKv3RaX/"
    ];
    private static readonly string[] YouTubeUrls =
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
#endif