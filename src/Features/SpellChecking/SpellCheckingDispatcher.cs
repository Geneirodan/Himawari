using Himawari.Core.Abstractions;
using Himawari.SpellChecking.Responses;
using Himawari.SpellChecking.Services;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Himawari.SpellChecking;

[PublicAPI]
public sealed class SpellCheckingDispatcher(IWrongLayoutParser parser, IServiceProvider serviceProvider) 
    : IMessageHandler
{
    public async Task OnMessage(Message msg, UpdateType update)
    {
        if (msg.Text is null || !parser.TryParse(msg.Text, out var correctedText))
            return;

        var message = new SendCorrectedTextMessageResponse(msg, correctedText);
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ISender>().Send(message).ConfigureAwait(false);
    }
}