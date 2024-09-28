using System.Globalization;
using Himawari.Abstractions;
using Himawari.Abstractions.Services;
using Himawari.Enums;
using Himawari.Extensions;
using Himawari.Models;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static Himawari.Resources.Games.Alias;

namespace Himawari.Commands;

public record AliasCommand(Message Message) : ICommand
{
    public class Handler(Bot bot, IAliasService service) : IRequestHandler<AliasCommand, Message>
    {
        public async Task<Message> Handle(AliasCommand request, CancellationToken cancellationToken)
        {
            var locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var chatId = request.Message.Chat.Id;
            service.Restart(chatId);
            var message = await bot.SendReplyMessage(
                message: request.Message,
                text: StartGame,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(
                        text: Want,
                        callbackData: new LocalizedCallback(Callback.AliasChoose, locale).Serialize()
                    )
                )
            ).ConfigureAwait(false);
            service.GetMessages(chatId).Add(message.MessageId);
            return message;
        }
    }
}