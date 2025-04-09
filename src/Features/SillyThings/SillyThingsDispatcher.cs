using Himawari.Core.Abstractions;
using Himawari.SillyThings.Responses;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.SillyThings;

[PublicAPI]
public sealed class SillyThingsDispatcher(IServiceProvider serviceProvider) : IMessageHandler
{
    public async Task OnMessage(Message msg, UpdateType update)
    {
        if (msg.Text is not { } messageText) return;

        using var scope = serviceProvider.CreateScope();

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        if (messageText.Contains("SS"))
        {
            await sender.Send(new SS.DetectedResponse(msg)).ConfigureAwait(false);
            await sender.Send(new SS.StickerResponse(msg)).ConfigureAwait(false);
        }

        if (messageText.Equals("какіш", StringComparison.InvariantCultureIgnoreCase))
            await sender.Send(new RhinoGifResponse(msg)).ConfigureAwait(false);
    }
}