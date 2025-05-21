using Himawari.Alias.Enums;
using Himawari.Alias.Extensions;
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
            var chatId = request.Message.Chat.Id;
            var message = await bot.SendMessage(
                chatId,
                text: Win,
                replyParameters: new ReplyParameters
                {
                    MessageId = request.Message.MessageId,
                    ChatId = chatId,
                    Quote = service.GetCurrentWord(chatId)
                },
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(
                        text: EndGame,
                        callbackData: AliasCallbackType.EndGame.Serialize()
                    ), 
                    InlineKeyboardButton.WithCallbackData(
                        text: Want,
                        callbackData: AliasCallbackType.Choose.Serialize()
                    )
                )
            ).ConfigureAwait(false);
            service.EndGame(chatId);
            return message;
        }
    }
}