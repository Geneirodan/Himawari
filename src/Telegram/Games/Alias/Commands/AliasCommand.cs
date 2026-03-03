using System.Globalization;
using Himawari.Alias.Enums;
using Himawari.Alias.Extensions;
using Himawari.Alias.Options;
using Himawari.Alias.Services;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.RateLimiting;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using static Himawari.Alias.Resources.Messages;

namespace Himawari.Alias.Commands;

/// <inheritdoc />
[BotCommand("/alias")]
public sealed record AliasCommand(Message Message) : ICommand, IChatAwareRequest
{
    /// <summary>Callback data prefix for category buttons. Telegram limit is 64 bytes; keep category keys short.</summary>
    private const string CategoryCallbackPrefix = "Alias-Category:";

    private const int CategoryButtonsPerRow = 2;

    /// <inheritdoc />
    public long ChatId => Message.Chat.Id;

    /// <summary>Starts the Alias game: sends category picker if configured (from AliasGame categories or WordSets), otherwise presenter picker. If a game is already running, replies with a warning.</summary>
    public sealed class Handler(IOutgoingTelegramBot bot, IAliasService service, IOptions<AliasWordSetsOptions> wordSetsOptions, IAliasWordService? wordService) : IRequestHandler<AliasCommand, Message>
    {
        public async Task<Message> Handle(AliasCommand request, CancellationToken cancellationToken)
        {
            var chatId = request.Message.Chat.Id;
            if (service.GetPresenterId(chatId) is not null)
                return await bot.SendReplyMessage(request.Message, GameAlreadyRunning, cancellationToken: cancellationToken).ConfigureAwait(false);

            var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
            var lang = culture.StartsWith("uk", StringComparison.OrdinalIgnoreCase) ? "uk" : culture.StartsWith("en", StringComparison.OrdinalIgnoreCase) ? "en" : "ru";

            if (wordService is not null && wordService.GetAllCategories().Count > 0)
            {
                var categoryButtons = wordService.GetAllCategories()
                    .Select(cat => InlineKeyboardButton.WithCallbackData(
                        cat.Labels.TryGetValue(lang, out var label) && !string.IsNullOrEmpty(label) ? label : cat.Key,
                        CategoryCallbackPrefix + cat.Key))
                    .ToArray();
                var rows = new List<InlineKeyboardButton[]>();
                for (var i = 0; i < categoryButtons.Length; i += CategoryButtonsPerRow)
                {
                    var row = categoryButtons.Skip(i).Take(CategoryButtonsPerRow).ToArray();
                    rows.Add(row);
                }
                return await bot.SendReplyMessage(
                    request.Message,
                    ChooseCategory,
                    replyMarkup: new InlineKeyboardMarkup(rows),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }

            var wordSets = wordSetsOptions.Value.WordSets;
            if (wordSets is not { Count: > 0 })
            {
                service.EndGame(chatId);
                return await bot.SendReplyMessage(
                    request.Message,
                    StartGame,
                    replyMarkup: new InlineKeyboardMarkup(InlineKeyboardButton.WithCallbackData(Want, AliasCallbackType.Choose.Serialize())),
                    cancellationToken: cancellationToken
                ).ConfigureAwait(false);
            }

            var fallbackButtons = wordSets.Keys
                .Select(key => InlineKeyboardButton.WithCallbackData(FormatCategoryLabel(key), CategoryCallbackPrefix + key))
                .ToArray();
            var fallbackRows = new List<InlineKeyboardButton[]>();
            for (var i = 0; i < fallbackButtons.Length; i += CategoryButtonsPerRow)
            {
                fallbackRows.Add(fallbackButtons.Skip(i).Take(CategoryButtonsPerRow).ToArray());
            }
            return await bot.SendReplyMessage(
                request.Message,
                ChooseCategory,
                replyMarkup: new InlineKeyboardMarkup(fallbackRows),
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
        }

        private static string FormatCategoryLabel(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;
            return char.ToUpper(key[0], CultureInfo.CurrentUICulture) + key[1..];
        }

        [PublicAPI]
        public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
            : AbstractCommandDescriptor<AliasCommand>(aliases.CurrentValue)
        {
            public override string Description => Resources.AliasCommand.Description;
            public override Func<Message, string, ICommand> Factory => (message, _) => new AliasCommand(message);
        }
    }
}