using System.Globalization;
using Himawari.Abstractions.Services;
using Himawari.Enums;
using Himawari.Models;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static Himawari.Resources.Games.Alias;

namespace Himawari.Games.Alias;

public record WinMessage(Message Message) : IRequest<Message>
{
    public class Handler(Bot bot, IAliasService service) : IRequestHandler<WinMessage, Message>
    {
        public async Task<Message> Handle(WinMessage request, CancellationToken cancellationToken)
        {
            var locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var chatId = request.Message.Chat.Id;
            var message = await bot.SendTextMessage(
                chatId,
                text: Win,
                replyParameters: new ReplyParameters
                {
                    MessageId = request.Message.MessageId,
                    ChatId = chatId,
                    Quote = await service.GetCurrentWordAsync(chatId,cancellationToken).ConfigureAwait(false)
                },
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(
                        text: EndGame,
                        callbackData: new LocalizedCallback(Callback.AliasRestart, locale).Serialize()
                    ), 
                    InlineKeyboardButton.WithCallbackData(
                        text: Want,
                        callbackData: new LocalizedCallback(Callback.AliasChoose, locale).Serialize()
                    )
                )
            ).ConfigureAwait(false);
            service.GetMessages(chatId).Add(message.MessageId);
            service.Restart(chatId);
            return message;
        }
    }
}