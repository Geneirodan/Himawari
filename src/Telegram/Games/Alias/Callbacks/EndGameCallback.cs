using System.Globalization;
using Himawari.Alias.Services;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.RateLimiting;
using MediatR;
using Telegram.Bot.Types;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

/// <summary>Callback when the user taps "end game": ends the Alias game for the chat and sends a confirmation message.</summary>
public sealed record EndGameCallback(CallbackQuery Query) : AbstractCallback<Message?>(Query)
{
    /// <inheritdoc />
    public sealed class Handler(Bot bot, IOutgoingTelegramBot outgoingBot, IAliasService service, IAliasRoundTimer roundTimer) : IRequestHandler<EndGameCallback, Message?>
    {
        public async Task<Message?> Handle(EndGameCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message?.Chat.Id is not { } chatId)
                return null;
            if (service.GetPresenterId(chatId) is null)
            {
                await bot.AnswerCallbackQuery(request.Query.Id, GameIsNotStarted, showAlert: true).ConfigureAwait(false);
                return null;
            }

            roundTimer.Cancel(chatId);
            var score = service.GetCorrectCount(chatId);
            service.EndGame(chatId);
            await bot.AnswerCallbackQuery(request.Query.Id).ConfigureAwait(false);
            var text = string.Format(CultureInfo.CurrentUICulture, GameEndedWithScore, score);
            return await outgoingBot.SendMessageAsync(chatId, text, cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }
}