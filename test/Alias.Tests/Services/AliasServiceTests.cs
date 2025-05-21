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
    private readonly AliasService _aliasService;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock = new();

    public AliasServiceTests()
    {
        var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _aliasService = new AliasService(httpClient);
    }

    [Fact]
    public async Task GetCurrentWordAsync_ShouldFetchNewWord_WhenNoWordExists()
    {
        const long chatId = 1;
        SetupHttpMessage()
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<span>test</span>")
                }
            );

        var word = await _aliasService.GetOrCreateCurrentWordAsync(chatId);

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
        const long chatId = 2;

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

        await _aliasService.GetOrCreateCurrentWordAsync(chatId);
        var word = await _aliasService.GetOrCreateCurrentWordAsync(chatId);

        word.ShouldNotBeNull();
        word.ShouldBe("test1");
    }

    [Fact]
    public async Task Restart_ShouldResetWordAndPresenterId()
    {
        SetupHttpMessage()
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<span>test1</span>")
                }
            );
        const long chatId = 3;
        await _aliasService.StartAsync(chatId, 67890L);
        _aliasService.EndGame(chatId);

        _aliasService.GetPresenterId(chatId).ShouldBe(null);
    }

    [Fact]
    public async Task GetPresenterId_ShouldReturnCorrectPresenterId()
    {
        SetupHttpMessage()
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("<span>test1</span>")
                }
            );
        const long chatId = 4;
        const long presenterId = 67890L;
        await _aliasService.StartAsync(chatId, presenterId);

        var result = _aliasService.GetPresenterId(chatId);

        result.ShouldBe(presenterId);
    }
}