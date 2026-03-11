using Himawari.SillyThings.Options;
using Himawari.SillyThings.Responses;
using Himawari.Telegram.Core.Abstractions;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Himawari.SillyThings;

/// <summary>
/// Message handler: detects sticker triggers (substring) and GIF triggers (exact match) via <see cref="SillyThingsTriggers"/>, sends <see cref="SSDetectedReply"/> or <see cref="RhinoGifReply"/>. Uses scoped <see cref="ISender"/> to send MediatR replies.
/// </summary>
[PublicAPI]
public sealed class SillyThingsDispatcher(IServiceProvider serviceProvider, SillyThingsTriggers triggers) : AbstractDispatcher
{
    /// <inheritdoc />
    protected override async Task OnNewMessage(Message msg)
    {
        if (msg.Text is not { } messageText) return;

        if (!triggers.ContainsAnyStickerTrigger(messageText.AsSpan()) && !triggers.ContainsAnyGifTrigger(messageText.AsSpan()))
            return;

        var scope = serviceProvider.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            if (triggers.ContainsAnyStickerTrigger(messageText.AsSpan()))
                await sender.Send(new SSDetectedReply(msg)).ConfigureAwait(false);

            if (triggers.ContainsAnyGifTrigger(messageText.AsSpan()))
                await sender.Send(new RhinoGifReply(msg)).ConfigureAwait(false);
        }
    }
}