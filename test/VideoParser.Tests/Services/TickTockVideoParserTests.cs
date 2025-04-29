using Himawari.VideoParser.Services;
using JetBrains.Annotations;
using Shouldly;
using Xunit;

namespace Himawari.VideoParser.Tests.Services;

[TestSubject(typeof(TikTokVideoParser))]
public class TikTokVideoParserTests
{
    private readonly TikTokVideoParser _parser;

    public TikTokVideoParserTests()
    {
        var client = new HttpClient();
        _parser = new TikTokVideoParser(client);
    }

    [Theory] [MemberData(nameof(MatchData))]
    public void ContainsUrl_ShouldReturnExpectedValue(string url, bool shouldMatch) => _parser.ContainsUrl(url).ShouldBe(shouldMatch);

    [Theory]
    [MemberData(nameof(ValidUrlsData))]
    public async Task GetInputFile_ShouldNotReturnNull_WhenUrlIsValid(string url)
    {
        var result = await _parser.GetInputFile(url);
        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldNotBeNull();
        result.Errors.ShouldBeEmpty();
    }

    [Theory]
    [MemberData(nameof(InvalidUrlsData))]
    public async Task GetInputFile_ShouldReturnNull_WhenUrlIsInvalid(string url)
    {
        var result = await _parser.GetInputFile(url); 
        result.IsSuccess.ShouldBeFalse();
        result.Errors.ShouldNotBeEmpty();
    }

    public static TheoryData<string, bool> MatchData()
    {
        var td =  new TheoryData<string, bool>();
        foreach (var url in ValidUrls) td.Add(url, true);
        foreach (var url in InvalidUrls) td.Add(url, false);
        return td;
    }
    
    public static TheoryData<string> ValidUrlsData()
    {
        var td =  new TheoryData<string>();
        foreach (var url in ValidUrls) td.Add(url);
        return td;
    } 
    
    public static TheoryData<string> InvalidUrlsData()
    {
        var td =  new TheoryData<string>();
        foreach (var url in InvalidUrls) td.Add(url);
        return td;
    }
    
    private static readonly string[] ValidUrls =
    [
       // "https://vm.tiktok.com/ZMBb5FeLg/",
        "https://www.tiktok.com/@bankai.games/video/7492817945027038486?_t=ZM-8vWPPtvHrcM&_r=1"
    ]; 
    
    private static readonly string[] InvalidUrls =
    [
        "https://vm.tiktok.com/",
        "https://www.tiktok.com/video/",
        "somesort of somtsf"
    ];
}