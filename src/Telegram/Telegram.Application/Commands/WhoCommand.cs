using Himawari.Telegram.Application.Resources;
using Himawari.Telegram.Core.Abstractions;
using Himawari.Telegram.Core.Abstractions.Messages;
using Himawari.Telegram.Core.Attributes;
using Himawari.Telegram.Core.Extensions;
using Himawari.Telegram.Core.Keyboards;
using Himawari.Telegram.Core.Localization;
using Himawari.Telegram.Core.Models;
using Himawari.Telegram.Core.RateLimiting;
using JetBrains.Annotations;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using WTelegram;

namespace Himawari.Telegram.Application.Commands;

/// <inheritdoc />
[BotCommand("/who")]
public sealed record WhoCommand(Message Message, string Rest) : ICommand
{
    public sealed class Handler(Bot bot, IOutgoingTelegramBot outgoingBot, IBotLocalizer loc, ILogger<Handler> logger) : IRequestHandler<WhoCommand, Message>
    {
        public async Task<Message> Handle(WhoCommand request, CancellationToken cancellationToken)
        {
            var (message, rest) = request;

            ChatMember[] members;
            try
            {
                var chatMembers = await bot.GetChatMemberList(message.Chat.Id).ConfigureAwait(false);
                members = chatMembers.Where(x => !x.User.IsBot).ToArray();
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "GetChatMemberList failed for chat {ChatId}", message.Chat.Id);
                return await outgoingBot.SendReplyMessage(message, loc[Loc.WhoCantListMembers], cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            if (members.Length == 0)
                return await outgoingBot.SendReplyMessage(message, loc[Loc.WhoNoMembers], cancellationToken: cancellationToken).ConfigureAwait(false);

            var users = members.Select(m => m.User).ToList();
            var keyboard = UserPickerKeyboard.Build(users, "who");
            return await outgoingBot.SendReplyMessage(
                message,
                loc[Loc.WhoPickPrompt],
                ParseMode.Html,
                replyMarkup: keyboard,
                cancellationToken: cancellationToken).ConfigureAwait(false);
        }
    }

    [PublicAPI]
    public sealed class Descriptor(IOptionsMonitor<Aliases> aliases)
        : AbstractCommandDescriptor<WhoCommand>(aliases.CurrentValue)
    {
        public override string Description => CommandDescriptions.Who;
        public override Func<Message, string, ICommand> Factory => (message, text) => new WhoCommand(message, text);
    }
}