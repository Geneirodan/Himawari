using Himawari.Telegram.Application.Resources;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Localization;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.RateLimiting;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Himawari.Telegram.Application.Commands;

/// <summary>
/// Handles /start: in private chat shows a persistent reply keyboard with quick commands; in groups does not reply to avoid noise (e.g. auto /start on member add).
/// </summary>
[BotCommand("/start")]
public sealed record StartCommand(Message Message) : ICommand
{
    private static readonly ReplyKeyboardMarkup QuickMenu = new(
    [
        [new KeyboardButton("/who"), new KeyboardButton("/gift")],
        [new KeyboardButton("/alias"), new KeyboardButton("/todo")],
        [new KeyboardButton("/help")]
    ])
    {
        ResizeKeyboard = true,
        InputFieldPlaceholder = "Выбери команду..."
    };

    /// <inheritdoc />
    public sealed class Handler(IOutgoingTelegramBot outgoingBot, IBotLocalizer loc) : IRequestHandler<StartCommand, Message?>
    {
        /// <inheritdoc />
        public async Task<Message?> Handle(StartCommand request, CancellationToken cancellationToken)
        {
            var message = request.Message;
            if (message.Chat.Type != ChatType.Private)
                return null;

            return await outgoingBot.SendReplyMessage(
                message,
                loc[Loc.StartGreeting],
                ParseMode.Html,
                QuickMenu,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<StartCommand>(aliases.CurrentValue)
    {
        /// <inheritdoc />
        public override string Description => CommandDescriptions.Start;
        /// <inheritdoc />
        public override Func<Message, string, ICommand> Factory => (message, _) => new StartCommand(message);
    }
}
