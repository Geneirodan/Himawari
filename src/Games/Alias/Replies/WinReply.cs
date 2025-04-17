using System.Globalization;
using Himawari.Alias.Models;
using Himawari.Alias.Services;
using Himawari.Core.Abstractions.Messages;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Replies;

public sealed record WinReply(Message Message) : IReply
{
    public sealed class Handler(Bot bot, IAliasService service) : IRequestHandler<WinReply, Message>
    {
        public async Task<Message> Handle(WinReply request, CancellationToken cancellationToken)
        {
            var locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var chatId = request.Message.Chat.Id;
            var message = await bot.SendMessage(
                chatId,
                text: Win,
                replyParameters: new ReplyParameters
                {
                    MessageId = request.Message.MessageId,
                    ChatId = chatId,
                    Quote = await service.GetCurrentWordAsync(chatId, cancellationToken).ConfigureAwait(false)
                },
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(
                        text: EndGame,
                        callbackData: new AliasCallbackData(AliasCallbackData.CallbackType.Restart, locale).Serialize()
                    ), 
                    InlineKeyboardButton.WithCallbackData(
                        text: Want,
                        callbackData: new AliasCallbackData(AliasCallbackData.CallbackType.Choose, locale).Serialize()
                    )
                )
            ).ConfigureAwait(false);
            service.Restart(chatId);
            return message;
        }
    }
}