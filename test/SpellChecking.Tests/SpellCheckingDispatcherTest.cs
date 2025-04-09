using Himawari.SpellChecking.Responses;
using Himawari.SpellChecking.Services;
using Himawari.TestHelpers;
using JetBrains.Annotations;
using MediatR;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.SpellChecking.Tests;

[TestSubject(typeof(SpellCheckingDispatcher))]
public class SpellCheckingDispatcherTest
{
    private readonly Mock<ISender> _sender = new();
    private readonly SpellCheckingDispatcher _dispatcher;
    private readonly Mock<IWrongLayoutParser> _wrongLayoutParser = new();

    public SpellCheckingDispatcherTest()
    {
        var serviceProvider = ServiceProviderMockFactory.CreateServiceProvider();
        serviceProvider
            .Setup(x => x.GetService(typeof(ISender)))
            .Returns(_sender.Object);
        _dispatcher = new SpellCheckingDispatcher(_wrongLayoutParser.Object, serviceProvider.Object);
    }

    [Fact]
    public async Task OnMessage_ShouldEarlyReturn_WhenMessageTextIsNull()
    {
        var message = new Message { Text = null };
        await _dispatcher.OnMessage(message, UpdateType.Message);
        _sender.Verify(
            x => x.Send(It.IsAny<SendCorrectedTextMessageResponse>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }

    [Fact]
    public async Task OnMessage_ShouldEarlyReturn_WhenParsingFailed()
    {
        var message = new Message { Text = "Good" };
        var output = string.Empty;
        _wrongLayoutParser.Setup(x => x.TryParse(It.IsAny<string>(), out output)).Returns(false);
        await _dispatcher.OnMessage(message, UpdateType.Message);
        _sender.Verify(
            x => x.Send(It.IsAny<SendCorrectedTextMessageResponse>(), It.IsAny<CancellationToken>()),
            Times.Never
        );
    }
    
    [Fact]
    public async Task OnMessage_ShouldReturnUpdatedText_WhenParsingSucceeded()
    {
        var message = new Message { Text = "Пщщв" };
        var output = "Good";
        _wrongLayoutParser.Setup(x => x.TryParse(It.IsAny<string>(), out output)).Returns(true);
        await _dispatcher.OnMessage(message, UpdateType.Message);
        _sender.Verify(
            x => x.Send(It.IsAny<SendCorrectedTextMessageResponse>(), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}