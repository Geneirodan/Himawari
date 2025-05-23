﻿using Himawari.Core.Abstractions;
using Himawari.SpellChecking.Responses;
using Himawari.SpellChecking.Services;
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
        using var scope = serviceProvider.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ISender>().Send(message).ConfigureAwait(false);
    }
}