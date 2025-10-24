using Himawari.SillyThings.Responses;
using Himawari.Telegram.Core.Abstractions;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Himawari.SillyThings;

[PublicAPI]
public sealed class SillyThingsDispatcher(IServiceProvider serviceProvider) : AbstractDispatcher
{
    protected override async Task OnNewMessage(Message msg)
    {
        if (msg.Text is not { } messageText) return;

        using var scope = serviceProvider.CreateScope();

        var sender = scope.ServiceProvider.GetRequiredService<ISender>();

        if (messageText.Contains("SS"))
            await sender.Send(new SSDetectedReply(msg)).ConfigureAwait(false);

        if (messageText.Equals("какіш", StringComparison.InvariantCultureIgnoreCase))
            await sender.Send(new RhinoGifReply(msg)).ConfigureAwait(false);
    }
}