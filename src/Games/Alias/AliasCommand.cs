using System.Globalization;
using Himawari.Core.Abstractions;
using Himawari.Core.Enums;
using Himawari.Core.Extensions;
using Himawari.Core.Models;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias;

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
            );
            service.GetMessages(chatId).Add(message.MessageId);
            return message;
        }
    }
}