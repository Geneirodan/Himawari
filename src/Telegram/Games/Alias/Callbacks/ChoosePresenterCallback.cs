using Himawari.Alias.Enums;
using Himawari.Alias.Extensions;
using Himawari.Alias.Services;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Extensions;
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

            await service.StartAsync(chatId, request.Query.From.Id, cancellationToken).ConfigureAwait(false);

            return await bot.SendMessage(
                chatId: chatId,
                text: string.Format(PresenterChosen, request.Query.From.GetUsername()),
                parseMode: ParseMode.MarkdownV2,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(EndGame, AliasCallbackType.EndGame.Serialize()),
                    InlineKeyboardButton.WithCallbackData(SeeWord, AliasCallbackType.SeeWord.Serialize()),
                    InlineKeyboardButton.WithCallbackData(NextWord, AliasCallbackType.NextWord.Serialize())
                )
            ).ConfigureAwait(false);
        }
    }
}