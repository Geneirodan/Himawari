using System.Globalization;
using System.Text.Encodings.Web;
using Himawari.Alias.Enums;
using Himawari.Alias.Extensions;
using Himawari.Alias.Services;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.RateLimiting;
using MediatR;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

/// <summary>Callback when the user chooses to be the presenter: starts the game, sends the word-control keyboard, and starts the round timer.</summary>
public sealed record ChoosePresenterCallback(CallbackQuery Query) : AbstractCallback<Message?>(Query)
{
    private static readonly TimeSpan RoundDuration = TimeSpan.FromSeconds(60);

    /// <inheritdoc />
    public sealed class Handler(Bot bot, IOutgoingTelegramBot outgoingBot, IAliasService service, IAliasRoundTimer roundTimer) : IRequestHandler<ChoosePresenterCallback, Message?>
    {
        public async Task<Message?> Handle(ChoosePresenterCallback request, CancellationToken cancellationToken)
        {
            var chatId = request.Query.Message!.Chat.Id;
            if (service.GetPresenterId(chatId) is not null)
            {
                await bot.AnswerCallbackQuery(request.Query.Id, PresenterAlreadyChosen, showAlert: true)
                    .ConfigureAwait(false);
                return null;
            }

            var category = service.GetCategory(chatId);
            await service.StartAsync(chatId, request.Query.From.Id, category, cancellationToken).ConfigureAwait(false);

            roundTimer.Start(chatId, RoundDuration, async (cid, ct) =>
            {
                var score = service.GetCorrectCount(cid);
                service.EndGame(cid);
                var text = string.Format(CultureInfo.CurrentUICulture, TimeUpWithScore, score);
                await outgoingBot.SendMessageAsync(cid, text, cancellationToken: ct).ConfigureAwait(false);
            });

            var usernameEncoded = HtmlEncoder.Default.Encode(request.Query.From.GetUsername() ?? string.Empty);
            return await outgoingBot.SendMessageAsync(
                chatId,
                string.Format(CultureInfo.CurrentUICulture, PresenterChosen, usernameEncoded),
                ParseMode.Html,
                new InlineKeyboardMarkup(
                    InlineKeyboardButton.WithCallbackData(CorrectButton, AliasCallbackType.Correct.Serialize()),
                    InlineKeyboardButton.WithCallbackData(SkipButton, AliasCallbackType.Skip.Serialize()),
                    InlineKeyboardButton.WithCallbackData(StopButton, AliasCallbackType.EndGame.Serialize())
                ),
                cancellationToken
            ).ConfigureAwait(false);
        }
    }
}