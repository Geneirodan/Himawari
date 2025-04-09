using System.Net;
using Himawari.Alias.Services;
using JetBrains.Annotations;
using Moq;
using Moq.Language;
using Moq.Protected;
using Shouldly;
using Xunit;

namespace Himawari.Alias.Tests.Services;

[TestSubject(typeof(AliasService))]
public class AliasServiceTests
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();
    private readonly AliasService _aliasService;

    public AliasServiceTests()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _aliasService = new AliasService(httpClient);
    }

    [Fact]
    public async Task GetCurrentWordAsync_ShouldFetchNewWord_WhenNoWordExists()
    {
        const long chatId = 12345L;
        SetupHttpMessage()
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<span>test</span>")
                }
            );

        var word = await _aliasService.GetCurrentWordAsync(chatId);

        word.ShouldNotBeNull();
        word.ShouldBe("test");
    }

    private ISetupSequentialResult<Task<HttpResponseMessage>> SetupHttpMessage() =>
        _httpMessageHandlerMock.Protected()
            .SetupSequence<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            );

    [Fact]
    public async Task GetCurrentWordAsync_ShouldReturnCachedWord_WhenWordExists()
    {
        const long chatId = 12345L;

        SetupHttpMessage()
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<span>test1</span>")
                }
            )
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<span>test2</span>")
                }
            );

        await _aliasService.GetCurrentWordAsync(chatId);
        var word = await _aliasService.GetCurrentWordAsync(chatId);

        word.ShouldNotBeNull();
        word.ShouldBe("test1");
    }

    [Fact]
    public void Restart_ShouldResetWordAndPresenterId()
    {
        const long chatId = 12345L;
        _aliasService.SetPresenterId(chatId, 67890L);

        _aliasService.Restart(chatId);

        _aliasService.GetPresenterId(chatId).ShouldBe(null);
    }

    [Fact]
    public void GetPresenterId_ShouldReturnCorrectPresenterId()
    {
        const long chatId = 12345L;
        const long presenterId = 67890L;
        _aliasService.SetPresenterId(chatId, presenterId);

        var result = _aliasService.GetPresenterId(chatId);

        result.ShouldBe(presenterId);
    }
}