using System.Globalization;
using Himawari.Core.Abstractions;
using Himawari.Core.Enums;
using Himawari.Core.Extensions;
using Himawari.Core.Models;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

public record ChoosePresenterCallback(CallbackQuery Query) : ICallback<Message?>
{
    public class Handler(Bot bot, IAliasService service) : IRequestHandler<ChoosePresenterCallback, Message?>
    {
        public async Task<Message?> Handle(ChoosePresenterCallback request, CancellationToken cancellationToken)
        {
            var chatId = request.Query.Message!.Chat.Id;
            if (service.GetPresenterId(chatId) is not null)
            {
                await bot.AnswerCallbackQuery(request.Query.Id, PresenterAlreadyChosen, true);
                return null;
            }
            service.SetPresenterId(chatId, request.Query.From.Id);

            var locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var message = await bot.SendTextMessage(
                chatId: chatId,
                text: string.Format(PresenterChosen, request.Query.From.GetUsername()),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(
                        text: EndGame,
                        callbackData: new LocalizedCallback(Callback.AliasRestart, locale).Serialize()
                    ), 
                    InlineKeyboardButton.WithCallbackData(
                        text: SeeWord,
                        callbackData: new LocalizedCallback(Callback.AliasSeeWord, locale).Serialize()
                    ), 
                    InlineKeyboardButton.WithCallbackData(
                        text: NextWord,
                        callbackData: new LocalizedCallback(Callback.AliasNextWord, locale).Serialize()
                    ))
            );
            service.GetMessages(chatId).Add(message.MessageId);
            return message;
        }
    }
}