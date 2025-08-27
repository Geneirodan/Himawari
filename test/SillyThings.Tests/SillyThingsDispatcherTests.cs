using Himawari.SillyThings.Responses;
using Himawari.TestHelpers;
using JetBrains.Annotations;
using MediatR;
using Moq;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace Himawari.SillyThings.Tests;

[TestSubject(typeof(SillyThingsDispatcher))]
public class SillyThingsDispatcherTests
{
    private readonly SillyThingsDispatcher _dispatcher;
    private readonly Mock<ISender> _sender = new();

    public SillyThingsDispatcherTests()
    {
        var serviceProvider = ServiceProviderMockFactory.CreateServiceProvider();
        serviceProvider
            .Setup(x => x.GetService(typeof(ISender)))
            .Returns(_sender.Object);
        _dispatcher = new SillyThingsDispatcher(serviceProvider.Object);
    }


    [Fact]
    public async Task OnMessage_ShouldEarlyReturn_WhenMessageTextIsNull()
    {
        var message = new Message { Text = null };
        await _dispatcher.OnMessage(message, UpdateType.Message);
        _sender.VerifyNoOtherCalls();
    }

    [Theory]
    [InlineData("SS")]
    [InlineData("SSAtStart")]
    [InlineData("AtEndSS")]
    public async Task OnMessage_ShouldReturnSsMessages_WhenSequenceDetected(string text)
    {
        var message = new Message { Text = text };
        await _dispatcher.OnMessage(message, UpdateType.Message);
        _sender.Verify(x => x.Send(It.IsAny<SSDetectedReply>(), It.IsAny<CancellationToken>()), Times.Once);
        _sender.Verify(x => x.Send(It.IsAny<RhinoGifReply>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task OnMessage_ShouldReturnRhinoGif_WhenSequenceDetected()
    {
        var message = new Message { Text = "какіш" };
        await _dispatcher.OnMessage(message, UpdateType.Message);
        _sender.Verify(x => x.Send(It.IsAny<SSDetectedReply>(), It.IsAny<CancellationToken>()), Times.Never);
        _sender.Verify(x => x.Send(It.IsAny<RhinoGifReply>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}