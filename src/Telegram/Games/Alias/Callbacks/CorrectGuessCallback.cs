using System.Globalization;
using Himawari.Alias.Enums;
using Himawari.Alias.Extensions;
using Himawari.Alias.Services;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.RateLimiting;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

/// <summary>Callback when the presenter taps "guessed": increments score, advances to next word, and edits the control message with updated text and the same inline keyboard.</summary>
public sealed record CorrectGuessCallback(CallbackQuery Query) : AbstractCallback<Message?>(Query)
{
    /// <inheritdoc />
    public sealed class Handler(Bot bot, IOutgoingTelegramBot outgoingBot, IAliasService service) : IRequestHandler<CorrectGuessCallback, Message?>
    {
        /// <inheritdoc />
        public async Task<Message?> Handle(CorrectGuessCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message?.Chat.Id is not { } chatId)
                return null;
            var messageId = request.Query.Message.MessageId;
            if (service.GetPresenterId(chatId) is not { } presenterId)
            {
                await bot.AnswerCallbackQuery(request.Query.Id, GameIsNotStarted, showAlert: true).ConfigureAwait(false);
                return null;
            }
            if (presenterId != request.Query.From.Id)
            {
                await bot.AnswerCallbackQuery(request.Query.Id, Forbidden, showAlert: true).ConfigureAwait(false);
                return null;
            }

            var count = service.IncrementCorrectCount(chatId);
            _ = await service.NextWordAsync(chatId, cancellationToken).ConfigureAwait(false);

            var scoreLine = string.Format(CultureInfo.CurrentUICulture, RoundScoreSuffix, count);
            var text = CorrectWordConfirmed + " " + scoreLine;
            var keyboard = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData(CorrectButton, AliasCallbackType.Correct.Serialize()),
                InlineKeyboardButton.WithCallbackData(SkipButton, AliasCallbackType.Skip.Serialize()),
                InlineKeyboardButton.WithCallbackData(StopButton, AliasCallbackType.EndGame.Serialize()));

            await bot.AnswerCallbackQuery(request.Query.Id).ConfigureAwait(false);
            return await outgoingBot.EditMessageTextAsync(chatId, messageId, text, ParseMode.Html, keyboard, cancellationToken).ConfigureAwait(false);
        }
    }
}
