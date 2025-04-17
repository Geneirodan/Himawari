using System.Globalization;
using Himawari.Alias.Models;
using Himawari.Alias.Services;
using Himawari.Core.Abstractions;
using Himawari.Core.Extensions;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

public sealed record ChoosePresenterCallback(CallbackQuery Query) : AbstractCallback<Message?>(Query)
{
    public sealed class Handler(Bot bot, IAliasService service) : IRequestHandler<ChoosePresenterCallback, Message?>
    {
        public async Task<Message?> Handle(ChoosePresenterCallback request, CancellationToken cancellationToken)
        {
            var chatId = request.Query.Message!.Chat.Id;
            if (service.GetPresenterId(chatId) is not null)
            {
                await bot.AnswerCallbackQuery(request.Query.Id, PresenterAlreadyChosen, true).ConfigureAwait(false);
                return null;
            }

            service.SetPresenterId(chatId, request.Query.From.Id);

            var locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var message = await bot.SendMessage(
                chatId: chatId,
                text: string.Format(PresenterChosen, request.Query.From.GetUsername()),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(
                        text: EndGame,
                        callbackData: new AliasCallbackData(AliasCallbackData.CallbackType.Restart, locale).Serialize()
                    ), 
                    InlineKeyboardButton.WithCallbackData(
                        text: SeeWord,
                        callbackData: new AliasCallbackData(AliasCallbackData.CallbackType.SeeWord, locale).Serialize()
                    ), 
                    InlineKeyboardButton.WithCallbackData(
                        text: NextWord,
                        callbackData: new AliasCallbackData(AliasCallbackData.CallbackType.NextWord, locale).Serialize()
                    ))
            ).ConfigureAwait(false);
            return message;
        }
    }
}