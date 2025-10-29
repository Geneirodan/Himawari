using Himawari.SpellChecking.Replies;
using Himawari.SpellChecking.Services;
using Himawari.Telegram.Core.Abstractions;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;

namespace Himawari.SpellChecking;

[PublicAPI]
public sealed class SpellCheckingDispatcher(IWrongLayoutParser parser, IServiceProvider serviceProvider)
    : AbstractDispatcher
{
    protected override async Task OnNewMessage(Message msg)
    {
        if (msg.Text is null || !parser.TryParse(msg.Text, out var correctedText))
            return;

        var message = new SendCorrectedTextMessageReply(msg, correctedText);
        var scope = serviceProvider.CreateAsyncScope();
        await using (scope.ConfigureAwait(false)) 
            await scope.ServiceProvider.GetRequiredService<ISender>().Send(message).ConfigureAwait(false);
    }
}