using System.Globalization;
using Himawari.Alias.Enums;
using Himawari.Alias.Extensions;
using Himawari.Alias.Services;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.RateLimiting;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Replies;

/// <summary>Reply sent when the user's guess is evaluated: correct or partial. Correct advances the game and shows score; partial ends the game with End/Want keyboard.</summary>
/// <param name="Message">The message that triggered the guess.</param>
/// <param name="IsCorrect">Whether the guess was correct.</param>
public sealed record GuessReply(Message Message, bool IsCorrect) : IReply
{
    /// <inheritdoc />
    public sealed class Handler(IOutgoingTelegramBot bot, IAliasService service, IAliasRoundTimer roundTimer) : IRequestHandler<GuessReply, IEnumerable<Message>>
    {
        public async Task<IEnumerable<Message>> Handle(GuessReply request, CancellationToken cancellationToken)
        {
            var chatId = request.Message.Chat.Id;
            if (request.IsCorrect)
            {
                var count = service.IncrementCorrectCount(chatId);
                _ = await service.NextWordAsync(chatId, cancellationToken).ConfigureAwait(false);
                var text = string.Format(CultureInfo.CurrentUICulture, RoundScoreSuffix, count);
                var message = await bot.SendReplyMessage(
                    request.Message,
                    CorrectWordConfirmed + " " + text,
                    replyParameters: new ReplyParameters
                    {
                        MessageId = request.Message.MessageId,
                        ChatId = chatId,
                        Quote = request.Message.Text
                    },
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
                return [message];
            }

            var endMessage = await bot.SendReplyMessage(
                request.Message,
                PartialGuess,
                replyMarkup: new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(text: EndGame, callbackData: AliasCallbackType.EndGame.Serialize()),
                    InlineKeyboardButton.WithCallbackData(text: Want, callbackData: AliasCallbackType.Choose.Serialize())
                ),
                replyParameters: new ReplyParameters
                {
                    MessageId = request.Message.MessageId,
                    ChatId = chatId,
                    Quote = request.Message.Text
                },
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
            roundTimer.Cancel(chatId);
            service.EndGame(chatId);
            return [endMessage];
        }
    }
}