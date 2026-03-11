using Himawari.Alias.Enums;
using Himawari.Alias.Extensions;
using Himawari.Alias.Options;
using Himawari.Alias.Services;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.RateLimiting;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using WTelegram;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Callbacks;

/// <summary>Callback when the user chooses a word category: stores the selection and shows the presenter picker (Who wants to be the presenter? + [I want]).</summary>
/// <param name="Query">The callback query from the inline button.</param>
/// <param name="CategoryKey">The selected category key (e.g. "animals", "random").</param>
public sealed record ChooseCategoryCallback(CallbackQuery Query, string CategoryKey) : AbstractCallback<Message?>(Query)
{
    /// <inheritdoc />
    public sealed class Handler(Bot bot, IOutgoingTelegramBot outgoingBot, IAliasService service, IOptions<AliasWordSetsOptions> wordSetsOptions, IAliasWordService? wordService) : IRequestHandler<ChooseCategoryCallback, Message?>
    {
        public async Task<Message?> Handle(ChooseCategoryCallback request, CancellationToken cancellationToken)
        {
            if (request.Query.Message?.Chat.Id is not { } chatId)
                return null;

            var fromWordSets = wordSetsOptions.Value.WordSets?.ContainsKey(request.CategoryKey) ?? false;
            var fromAliasGame = wordService?.HasCategory(request.CategoryKey) ?? false;
            if (!fromWordSets && !fromAliasGame)
            {
                await bot.AnswerCallbackQuery(request.Query.Id, "Unknown category.", showAlert: true).ConfigureAwait(false);
                return null;
            }

            service.SetCategory(chatId, request.CategoryKey);
            await bot.AnswerCallbackQuery(request.Query.Id).ConfigureAwait(false);

            var messageId = request.Query.Message.MessageId;
            var keyboard = new InlineKeyboardMarkup(
                InlineKeyboardButton.WithCallbackData(Want, AliasCallbackType.Choose.Serialize()));
            return await outgoingBot.EditMessageTextAsync(
                chatId,
                messageId,
                StartGame,
                ParseMode.Html,
                keyboard,
                cancellationToken
            ).ConfigureAwait(false);
        }
    }
}
