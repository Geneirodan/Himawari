﻿using Himawari.Alias.Services;
using Himawari.Core.Abstractions;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

public sealed record EndGameCallback(CallbackQuery Query) : AbstractCallback<Message?>(Query)
{
    public sealed class Handler(Bot bot, IAliasService service) : IRequestHandler<EndGameCallback, Message?>
    {
        public async Task<Message?> Handle(EndGameCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message is not { } message)
                return null;

            var chatId = message.Chat.Id;
            if (service.GetPresenterId(chatId) is null)
            {
                await bot.AnswerCallbackQuery(request.Query.Id, GameIsNotStarted, true).ConfigureAwait(false);
                return null;
            }

            service.Restart(chatId);
            return await bot.SendMessage(chatId, GameEnded).ConfigureAwait(false);
        }
    }
}